#!/usr/bin/perl
# -----------------------------------------------------------------------------
#  version.pl — the ONE versioner, identical in every Hawkynt repo.
#
#  Model: FILES drive versions, never git tags.
#    * .NET repos: each .csproj carries its OWN <Version> (the per-package base).
#      This is deliberate — different NuGet packages carry different versions,
#      bumped independently as each package changes. This script appends the git
#      PER-PROJECT build segment and stamps <Version>BASE.BUILD</Version> back
#      into EACH csproj. BASE and BUILD are both independent per package, e.g.
#      A at 1.0.2.<commits-touching-A> and B at 2.3.0.<commits-touching-B>.
#    * Non-.NET repos (no .csproj anywhere): fall back to a plain VERSION file at
#      the repo root. There is nothing to stamp; --list just reports VERSION.BUILD.
#      (This is the ONLY place a VERSION file is used — .NET repos never need one.)
#
#  BUILD = commits touching the project's directory
#          (`git rev-list --count HEAD -- <project-dir>`); repo-wide for --build.
#
#  Usage:
#    perl version.pl --stamp   # rewrite <Version> in every csproj to BASE.BUILD
#    perl version.pl --build   # print the build number (commit count) only
#    perl version.pl --list    # print "<path>\t<BASE.BUILD>" per csproj (or VERSION)
#
#  Exit: 0 success, 2 bad usage.
# -----------------------------------------------------------------------------
use strict;
use warnings;
use FindBin;
use File::Find;
use File::Copy;

my $mode = $ARGV[0] // '--stamp';
exit 2 unless $mode eq '--stamp' || $mode eq '--build' || $mode eq '--list';

my $root  = _RepoRoot("$FindBin::Bin");

# --build prints the repo-wide commit count (the only context-free build number).
if ($mode eq '--build') { print _BuildNumber($root), "\n"; exit 0; }

my @csprojs = _Csprojs($root);

if ($mode eq '--list') {
    if (@csprojs) {
        for my $f (@csprojs) {
            my $b = _ReadVersion($f);
            next unless defined $b;
            # BUILD is PER-PROJECT: commits touching this csproj's directory.
            print "$f\t" . _Compose($b, _BuildNumber($root, _ProjDir($root, $f))) . "\n";
        }
    } else {
        my $b = _VersionFile($root);
        print "VERSION\t" . _Compose($b, _BuildNumber($root)) . "\n" if defined $b;
    }
    exit 0;
}

# --stamp
unless (@csprojs) {
    my $b = _VersionFile($root);
    print defined $b
        ? "no csproj; VERSION-based version is " . _Compose($b, _BuildNumber($root)) . " (nothing to stamp)\n"
        : "no csproj and no VERSION file; nothing to stamp\n";
    exit 0;
}

my $n = 0;
for my $f (@csprojs) {
    my $b = _ReadVersion($f);
    unless (defined $b) { warn "[warn] no <Version> in $f; skipped\n"; next; }
    # Each package's BUILD segment counts only commits that touched ITS directory,
    # so independently-changed packages get independent build numbers.
    $n++ if _Rewrite($f, _Compose($b, _BuildNumber($root, _ProjDir($root, $f))));
}
print "stamped $n csproj file(s) with per-project build numbers\n";
exit 0;

# --------------------------------------------------------------------------- #

# base (tidied to <=3 numeric segments) + ".BUILD"
sub _Compose {
    my ($base, $build) = @_;
    return undef unless defined $base;
    my @p = split /\./, $base;
    @p = @p[0 .. 2] if @p > 3;
    return join('.', @p) . ".$build";
}

sub _RepoRoot {
    my ($d) = @_;
    for (1 .. 20) {
        return $d if -d "$d/.git";
        my $p = $d;
        $p =~ s{[/\\][^/\\]+$}{};
        last if $p eq $d || $p eq '';
        $d = $p;
    }
    return $d;
}

sub _BuildNumber {
    my ($r, $rel) = @_;
    # With a relative project path, count only commits that touched it.
    my $spec = (defined $rel && length $rel) ? " -- \"$rel\"" : "";
    my $c = `git -C "$r" rev-list --count HEAD$spec 2>&1`;
    chomp $c;
    return $c =~ /^\d+$/ ? $c : '0';
}

# Path of a csproj's directory, relative to the repo root ('' = repo root).
sub _ProjDir {
    my ($root, $file) = @_;
    (my $dir = $file) =~ s{[/\\][^/\\]+$}{};   # strip the filename
    (my $r   = $root) =~ s{[/\\]$}{};
    $dir =~ s{^\Q$r\E[/\\]?}{};                # make relative to repo root
    return $dir;
}

sub _Csprojs {
    my ($r) = @_;
    my @f;
    my %skip = map { $_ => 1 } qw(
        bin obj packages node_modules .git .vs .idea TestResults
        artifacts publish dist stage coverage
    );
    File::Find::find(
        {
            no_chdir   => 1,
            preprocess => sub { grep { !$skip{$_} } @_ },
            wanted     => sub { push @f, $File::Find::name if /\.csproj$/i && -f $File::Find::name },
        },
        $r,
    );
    return sort @f;
}

sub _ReadVersion {
    my ($f) = @_;
    open my $fh, '<', $f or return undef;
    while (my $l = <$fh>) {
        if ($l =~ m{<Version>\s*([\d.]+)\s*</Version>}i) { close $fh; return $1; }
    }
    close $fh;
    return undef;
}

sub _VersionFile {
    my ($r) = @_;
    my $vf = "$r/VERSION";
    return undef unless -r $vf;
    open my $fh, '<', $vf or return undef;
    chomp(my $v = <$fh>);
    close $fh;
    $v =~ s/^\s+|\s+$//g if defined $v;
    return (defined $v && length $v) ? $v : undef;
}

sub _Rewrite {
    my ($f, $full) = @_;
    my $tmp = "$f.\$\$\$";
    open my $in,  '<', $f   or die "read $f: $!";
    open my $out, '>', $tmp or die "write $tmp: $!";
    my $hit = 0;
    while (my $l = <$in>) {
        $hit = 1 if $l =~ s{<Version>\s*[\d.]+\s*</Version>}{<Version>$full</Version>}i;
        print $out $l;
    }
    close $out;
    close $in;
    if ($hit) { File::Copy::move($tmp, $f) or die "replace $f: $!"; return 1; }
    unlink $tmp;
    return 0;
}
