#!/usr/bin/perl
# -----------------------------------------------------------------------------
#  version.pl — the ONE versioner, identical in every Hawkynt repo.
#
#  Model: FILES drive versions, never git tags.
#    * .NET repos: each .csproj carries its OWN <Version> (the per-package base).
#      This is deliberate — different NuGet packages carry different versions,
#      bumped independently as each package changes. This script appends the git
#      commit count as the build segment and stamps <Version>BASE.BUILD</Version>
#      back into EACH csproj. Package A at 1.0.2 and package B at 2.3.0 stay
#      independent; both just gain the shared .BUILD tail.
#    * Non-.NET repos (no .csproj anywhere): fall back to a plain VERSION file at
#      the repo root. There is nothing to stamp; --list just reports VERSION.BUILD.
#      (This is the ONLY place a VERSION file is used — .NET repos never need one.)
#
#  BUILD = `git rev-list --count HEAD`.
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
my $build = _BuildNumber($root);

if ($mode eq '--build') { print "$build\n"; exit 0; }

my @csprojs = _Csprojs($root);

if ($mode eq '--list') {
    if (@csprojs) {
        for my $f (@csprojs) {
            my $b = _ReadVersion($f);
            print "$f\t" . _Compose($b, $build) . "\n" if defined $b;
        }
    } else {
        my $b = _VersionFile($root);
        print "VERSION\t" . _Compose($b, $build) . "\n" if defined $b;
    }
    exit 0;
}

# --stamp
unless (@csprojs) {
    my $b = _VersionFile($root);
    print defined $b
        ? "no csproj; VERSION-based version is " . _Compose($b, $build) . " (nothing to stamp)\n"
        : "no csproj and no VERSION file; nothing to stamp\n";
    exit 0;
}

my $n = 0;
for my $f (@csprojs) {
    my $b = _ReadVersion($f);
    unless (defined $b) { warn "[warn] no <Version> in $f; skipped\n"; next; }
    $n++ if _Rewrite($f, _Compose($b, $build));
}
print "stamped $n csproj file(s) at build $build\n";
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
    my ($r) = @_;
    my $c = `git -C "$r" rev-list --count HEAD 2>&1`;
    chomp $c;
    return $c =~ /^\d+$/ ? $c : '0';
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
