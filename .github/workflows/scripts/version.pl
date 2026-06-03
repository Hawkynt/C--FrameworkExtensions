#!/usr/bin/perl
# -----------------------------------------------------------------------------
#  version.pl — the ONE versioner, identical in every Hawkynt repo.
#
#  Model: FILES drive versions, never git tags. The script scans for package
#  manifests across ecosystems and, for EACH manifest, computes a build number
#  from the commit count of that manifest's PARENT FOLDER, then stamps it back
#  into the manifest. So every package versions independently, by how much its
#  own directory actually changed.
#
#  Manifests recognised (kind -> file -> version field):
#    dotnet : *.csproj / *.fsproj / *.vbproj   <Version>X.Y.Z</Version>
#    node   : package.json                      "version": "X.Y.Z"
#    php    : composer.json                     "version": "X.Y.Z"
#    perl   : *.pm that declares $VERSION        our $VERSION = 'X.Y.Z';
#  A *.pm without a $VERSION is ignored, so only real package modules match.
#  Repos with no manifest (e.g. Go — versioned by tags) are left untouched.
#
#  BUILD = `git rev-list --count HEAD -- <manifest's parent dir>` (repo-wide for
#  --build). Composition respects each ecosystem's version grammar:
#    dotnet / perl : X.Y.Z.BUILD   (4-part is legal there)
#    node / php    : X.Y.Z+BUILD   (semver forbids a 4th part; build-metadata)
#
#  Usage:
#    perl version.pl --stamp   # rewrite the version in every manifest
#    perl version.pl --build   # print the repo-wide build number (commit count)
#    perl version.pl --list    # print "<manifest>\t<composed-version>" per package
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

my $root = _RepoRoot("$FindBin::Bin");

# --build prints the repo-wide commit count (the only context-free build number).
if ($mode eq '--build') { print _BuildNumber($root), "\n"; exit 0; }

my @manifests = _Manifests($root);

if ($mode eq '--list') {
    for my $m (@manifests) {
        my ($file, $kind) = @$m;
        my $base = _ReadBase($kind, $file);
        next unless defined $base;
        my $build = _BuildNumber($root, _ParentDir($root, $file));
        print "$file\t" . _Compose($kind, $base, $build) . "\n";
    }
    exit 0;
}

# --stamp
my $n = 0;
for my $m (@manifests) {
    my ($file, $kind) = @$m;
    my $base = _ReadBase($kind, $file);
    next unless defined $base;
    # Build segment counts only commits that touched this manifest's directory,
    # so independently-changed packages get independent build numbers.
    my $full = _Compose($kind, $base, _BuildNumber($root, _ParentDir($root, $file)));
    $n++ if _Rewrite($kind, $file, $full);
}
print "stamped $n manifest(s) with per-package build numbers\n";
exit 0;

# --------------------------------------------------------------------------- #

# Compose the ecosystem-appropriate version string from base + build.
sub _Compose {
    my ($kind, $base, $build) = @_;
    return undef unless defined $base;
    my @p = split /\./, $base;
    @p = @p[0 .. 2] if @p > 3;
    my $core = join('.', @p);
    # semver (node/php) forbids a 4th numeric part -> put build in metadata.
    return ($kind eq 'node' || $kind eq 'php') ? "$core+$build" : "$core.$build";
}

sub _Kind {
    my ($f) = @_;
    return 'dotnet' if $f =~ /\.(?:csproj|fsproj|vbproj)$/i;
    return 'node'   if $f =~ m{(?:^|[/\\])package\.json$}i;
    return 'php'    if $f =~ m{(?:^|[/\\])composer\.json$}i;
    return 'perl'   if $f =~ /\.pm$/i;
    return undef;
}

sub _Manifests {
    my ($r) = @_;
    my @out;
    my %skip = map { $_ => 1 } qw(
        bin obj packages node_modules .git .vs .idea TestResults
        artifacts publish dist stage coverage vendor .svn
    );
    File::Find::find(
        {
            no_chdir   => 1,
            preprocess => sub { grep { !$skip{$_} } @_ },
            wanted     => sub {
                my $f = $File::Find::name;
                return unless -f $f;
                my $kind = _Kind($f);
                push @out, [$f, $kind] if $kind;
            },
        },
        $r,
    );
    return sort { $a->[0] cmp $b->[0] } @out;
}

# ---- per-kind version readers (return the base X.Y.Z, or undef) ------------

sub _ReadBase {
    my ($kind, $f) = @_;
    return _ReadDotnet($f) if $kind eq 'dotnet';
    return _ReadJson($f)   if $kind eq 'node' || $kind eq 'php';
    return _ReadPerl($f)   if $kind eq 'perl';
    return undef;
}

sub _Slurp {
    my ($f) = @_;
    open my $fh, '<', $f or return undef;
    local $/;
    my $c = <$fh>;
    close $fh;
    return $c;
}

sub _ReadDotnet {
    my ($f) = @_;
    my $c = _Slurp($f) // return undef;
    return $c =~ m{<Version>\s*(\d+(?:\.\d+){0,2})\s*</Version>}i ? $1 : undef;
}

sub _ReadJson {
    my ($f) = @_;
    my $c = _Slurp($f) // return undef;
    return $c =~ m{"version"\s*:\s*"v?(\d+(?:\.\d+){0,2})}i ? $1 : undef;
}

sub _ReadPerl {
    my ($f) = @_;
    my $c = _Slurp($f) // return undef;
    return $c =~ m{\$VERSION\s*=\s*['"]v?(\d+(?:\.\d+){0,3})}i ? $1 : undef;
}

# ---- per-kind rewriters (return 1 if the file changed) ---------------------

sub _Rewrite {
    my ($kind, $f, $full) = @_;
    my $c = _Slurp($f);
    return 0 unless defined $c;
    my $orig = $c;
    if ($kind eq 'dotnet') {
        $c =~ s{<Version>\s*[\w.+\-]+\s*</Version>}{<Version>$full</Version>}ig;
    } elsif ($kind eq 'node' || $kind eq 'php') {
        $c =~ s{("version"\s*:\s*")[^"]*(")}{$1$full$2}i;   # first occurrence only
    } elsif ($kind eq 'perl') {
        $c =~ s{(\$VERSION\s*=\s*['"])[^'"]*(['"])}{$1$full$2};
    } else {
        return 0;
    }
    return 0 if $c eq $orig;
    my $tmp = "$f.\$\$\$";
    open my $out, '>', $tmp or die "write $tmp: $!";
    print $out $c;
    close $out;
    File::Copy::move($tmp, $f) or die "replace $f: $!";
    return 1;
}

# ---- git / path helpers ----------------------------------------------------

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
    my $spec = (defined $rel && length $rel) ? " -- \"$rel\"" : "";
    my $c = `git -C "$r" rev-list --count HEAD$spec 2>&1`;
    chomp $c;
    return $c =~ /^\d+$/ ? $c : '0';
}

# Path of a manifest's directory, relative to the repo root ('' = repo root).
sub _ParentDir {
    my ($root, $file) = @_;
    (my $dir = $file) =~ s{[/\\][^/\\]+$}{};   # strip the filename
    (my $r   = $root) =~ s{[/\\]$}{};
    $dir =~ s{^\Q$r\E[/\\]?}{};                # make relative to repo root
    return $dir;
}
