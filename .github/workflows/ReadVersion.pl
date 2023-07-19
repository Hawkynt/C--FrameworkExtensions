#!/usr/bin/perl
use strict;
use warnings;

sub main(@) {
  my ($inputFile) = @_;
  print "[Info]Grabbing version from file $inputFile\n";
  die "[Error]Input file not set. Usage $0 <projectFile>" unless $inputFile;
  die "[Error]Could not find file $inputFile" unless -f $inputFile;
  my $version = QueryVersionFromFile($inputFile) || '1.0.0.0';
  print "[Info]Using version $version\n";
  my $gitVersion=QueryGitVersion();
  print "[Info]Using git version $gitVersion\n";
  WriteToEnvironment(($ENV{GITHUB_ENV} || 'env'), <<EOF
Project_Version=$version
Git_Count=$gitVersion
Compile_Version=$version.$gitVersion
EOF
  );
}

sub QueryVersionFromFile($){
  my ($inputFile) = @_;
  open FH,"<",$inputFile or die "[Error] Could not open file $inputFile";
    while(my $line=<FH>){
      if($line =~ /<version>\s*([\d\.]+)\s*<\/version>/i){
        close FH;
        my ($version) = $line =~ /<version>\s*([\d\.]+)\s*<\/version>/i;
        print "[Info]Found version $version\n";
        return $version;
      }
    }
  close FH;
  return undef;
}

sub WriteToEnvironment($$){
  my ($outputFile, $text) = @_;
  open FH,">>",$outputFile or die "[Error]Could not write environment file $outputFile";
    print FH $text;
  close FH;
}

sub QueryGitVersion(){
  my $result=`git rev-list HEAD --count`;
  chomp $result;
  return $result;
}

main(@ARGV);
