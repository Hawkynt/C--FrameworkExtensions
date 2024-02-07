#!/usr/bin/perl
use strict;
use warnings;

use Cwd;
use File::Basename;
use File::Copy;
use File::Find;

sub main(@) {
  my($rootDirectoryName)=@_;
  die "[Error]Usage $0 <rootDirectory>\n" unless $rootDirectoryName;
  my $realName=Cwd::realpath($rootDirectoryName);
  die "[Error]Could not parse directory: $!, $rootDirectoryName\n" unless $realName;
  die "[Error]Directory not found: $realName\n" unless -d $realName;

  print "[Info]Querying for project files...";
  my $projectFiles=_FindProjectFiles($realName);
  print "Found ".scalar(@$projectFiles)."\n";
  
  print "[Info]Retrieving versions\n";
  my $versions=_QueryVersions($projectFiles);
  
  my $removedFiles=scalar(@$projectFiles)-scalar(keys(%$versions));
  print "[Warning]$removedFiles files did not have a version tag and must not be referenced!" if($removedFiles>0);
  
  _UpdateVersions($versions);
}

sub _DebugDump($){
  my ($var)=@_;
  require Data::Dumper;
  my $dumper=Data::Dumper->new([$var],["var"]);
  $dumper->Indent(1);
  $dumper->Useqq(1);
  $dumper->Sortkeys(1);
  print $dumper->Dump();
}

sub _UpdateVersions($) {
  my($filesWithVersion)=@_;
  foreach my $fileName (keys(%$filesWithVersion)) {
    my $version=$filesWithVersion->{$fileName};
    my $outputFileName="$fileName.\$\$\$";
    open my $fh,"<",$fileName or die "[Error]Could not read file $fileName:$!";
      open my $fo,">",$outputFileName or die "[Error]Could not write file $outputFileName:$!";
        while(my $line=<$fh>) {
          $line="<version>$version</version>\n" if($line =~ /<version>\s*([\d\.]+)\s*<\/version>/i);
          print $fo $line;
        }
      close $fo;
    close $fh;
    File::Copy::move($outputFileName,$fileName) or die "[Error]Could not replace file $fileName:$!";
  }
}

sub _QueryVersions($) {
  my($fileNames)=@_;
  my %results;
  foreach my $fileName (@$fileNames) {
    my $directoryName=File::Basename::dirname($fileName);
    my $version = _QueryVersionFromFile($fileName);
    unless ($version) {
      print "[Warning]No version tag present in $fileName\n";
      next;
    }

    # tidy if they have more than three segments
    $version =~ s/^(\d+)(\.\d+)?(\.\d+)?.*/$1$2$3/;

    my $commitCount=_QueryGitCommitCount($directoryName);
    $results{$fileName}="$version.$commitCount";
    print "[Info]Using base version $version -> $results{$fileName}\n";
  }
  return \%results;
}

sub _QueryVersionFromFile($){
  my ($inputFileName) = @_;
  open my $fh,"<",$inputFileName or die "[Error]Could not read file $inputFileName:$!\n";
    while(my $line=<$fh>){
      next unless($line =~ /<version>\s*([\d\.]+)\s*<\/version>/i);
      
      close $fh;
      my ($result) = $line =~ /<version>\s*([\d\.]+)\s*<\/version>/i;
      print "[Verbose]Found version in file $inputFileName: $result\n";
      return $result;
    }
  close $fh;
  return undef;
}

sub _QueryGitCommitCount(;$) {
  my ($workingCopyFileOrFolder)=@_;
  my $command = "git rev-list HEAD --all --branches --full-history --count $workingCopyFileOrFolder";
  print "[Verbose]Executing git> $command\n";
  my $result=`$command`;
  chomp $result;
  return $result;
}


sub _FindProjectFiles($) {
  my($rootDirectoryName)=@_;
  my @results;
  File::Find::find(sub{
    my $current=$File::Find::name;
    push(@results,$current) if ($current=~/\.csproj$/i && -f $current);
  },$rootDirectoryName);
  return \@results;
}

main(@ARGV);