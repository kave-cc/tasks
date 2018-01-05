####################### Configuration ##########################

$validYears = "2013","2014","2015","2016","2017","2013-2015"
$validNames = "Technische Universität Darmstadt","Sebastian Proksch", "University of Zurich", "Nico Strebel"

$root = "../"
$excludeFiles = "*.generated.cs","*.designer.cs","*TinyMessenger.cs", "*AssemblyInfo.cs"
$excludeFolders = "*\obj\*","*\bin\*","*\packages\*","*\Packages\*","*\test\data\*", "*\.git\*"

####################### Implementation #########################

$root = (Get-Item $root).FullName
"Scanning for invalid license headers in '$root'..."

function Get-ContentAsString ($file)
{
  return (Get-Content $file) -join "`n"
}

filter Where-NotMatch([String[]]$Like) 
{
    if ($Like.Length) {
        foreach ($Pattern in $Like) {
            if ($_ -like $Pattern) { return }
        }
    }
    return $_
} 

function IsInValidCopyrightString ($copyright)
{   
    $indexOfYear = $copyright.indexof(" ") + 1
    $indexOfYear = $copyright.indexof(" ") + 1
    $indexOfName = $copyright.indexof(" ",$indexofYear) + 1
    $year = $copyright.substring($indexOfYear,$indexOfName-$indexOfYear-1)
    $name = $copyright.substring($indexOfName)
    $isInvalidYear = !($validYears -contains $year)
    $isInvalidName = !($validNames -contains $name)

    return ($isInvalidYear -or $isInvalidName)
}

function ReplaceCopyrightStringInHeader ($header,$copyrightString) 
{
    $indexOfCopyright = $header.IndexOf("Copyright")
    $headerCopyrightString = ($header.substring($indexOfCopyright)).split("`n")[0]

    $header = $header.replace($headerCopyrightString,"")
    $header = $header.insert($indexOfCopyright,$copyrightString)

    return $header    
}

$global:allFilesContainValidHeader = $TRUE
function Error($fileShort, $message)
{
    Write-Host "${fileShort}: ${message}"
    $global:allFilesContainValidHeader = $FALSE
}

function Verify-Copyright-Line($fileShort, $line)
{
    $indexOfCopyright = $line.indexof("Copyright")
    $indexOfYear = $line.indexof(" ", $indexOfCopyright) + 1
    $indexOfName = $line.indexof(" ",$indexofYear) + 1

    $year = $line.substring($indexOfYear,$indexOfName-$indexOfYear-1)
    $name = $line.substring($indexOfName)

    if(!($validYears -contains $year))
    {
        Error $fileShort "invalid year '$year'"
    }

    if(!($validNames -contains $name))
    {
        Error $fileShort "invalid name '$name'"
    }
}

function Verify-Header ($file,$fileShort,$header)
{
    $content = Get-ContentAsString $file
    
    # Split expected file header by line breaks
    $headerLineSplit = $header.split("`n")
    
    $fileStartsWithExpComment = $content.startswith($header.split("`n")[0])
    if(!$fileStartsWithExpComment) 
    {
        Error $fileShort "does not start with comment"
    }

    $headerStart = $content.IndexOf($headerLineSplit[0])
    $headerEnd = $content.IndexOf($headerLineSplit[-1])
    $fileHeader = $content.substring($headerStart, $headerEnd + $headerLineSplit[-1].length)
    $fileHeaderLineSplit = $fileHeader.split("`n")

    if(! ($headerLineSplit.length -eq $fileHeaderLineSplit.length))
    {
        Error $fileShort "comment lengths differ"
    }

    for($i = 0; $i -lt $headerLineSplit.length; $i++)
    {
        $exp = $headerLineSplit[$i]
        $act = $fileHeaderLineSplit[$i]

        if($exp.contains("%COPYRIGHT_HOLDER%"))
        {
            Verify-Copyright-Line $fileShort $act
        }
        else
        {
            if(! ($act -eq $exp))
            {
                Error $fileShort "line $i differs from license template ('$act' vs. '$exp')"
            }
        }
    }
}

function Validate-File-Headers($pattern,$licenseHeader)
{
    $robocopyArguments = "/L","/S","/FP","/NDL","/NS","/NC","/NJS","/NJH"
    
    $files = robocopy $root NULL $robocopyArguments `
        | ?{$_ -like $pattern } `
        | Where-NotMatch $excludeFolders `
        | Where-NotMatch $excludeFiles `
        | % {$_.Trim() }

    ForEach ($file in $files)
    {
        $fileShort = $file.Substring($root.Length + 1)
        Verify-Header $file $fileShort $licenseHeader
    }
}

$license = Get-Item ./licenseheaders.txt
$header = Get-ContentAsString $license
$csLicenseHeader = $header.substring($header.IndexOf("/*"),$header.LastIndexOf("*/") - $header.IndexOf("/*") + 2)
$xamlLicenseHeader = $header.substring($header.IndexOf("<!--"),$header.LastIndexOf("-->") - $header.IndexOf("<!--") + 3)

Validate-File-Headers "*.cs" $csLicenseHeader
Validate-File-Headers "*.xml" $xamlLicenseHeader
Validate-File-Headers "*.xaml" $xamlLicenseHeader

if(!($global:allFilesContainValidHeader))
{
    Write-Error "Some files do not contain a valid license header."
    exit 1
}
else
{
    Write-Host "All files contain a valid license header."
}
