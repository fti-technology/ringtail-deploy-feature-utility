# Feature CSV file generator
#
# This script is responsible for looping over all txt files
# in a directory specified as a command line argument.
# It will split the file names on the underscore and expects exactly 2-3 parts
# FEATUREKEYNAME_TYPE_MINORKEY.txt
# The feature description should be a short summary placed inside the file itself.
#
param(
[Parameter(Mandatory=$true)][string]$FeaturesFileDir,
[Parameter(Mandatory=$true)][string]$OutPutDir
)

$OutPutDir = $OutPutDir.trimend("`"")

Write-Host "Looking for feature files in : " $FeaturesFileDir
Write-Host $OutPutDir
$returnStatus = 0;
$files = Get-ChildItem $FeaturesFileDir -Filter *.txt
$csvContents = @()
Write-Host "File found in directory: " + $files.Count
for ($i=0; $i -lt $files.Count; $i++) {
    
    #get the file name
    $outfile = $files[$i].FullName
    #get the file extension and split on the underscore
    $fileName = [System.IO.Path]::GetFileNameWithoutExtension($outfile)
    $splitArr = $fileName -split '_'
    
    if($splitArr.Count -ge 2){

        # Read in the description
        $desc = Get-Content $outfile
        
        # Create a new custom object to hold our result.
        $featureObject = new-object PSObject
        $featureObject | add-member -membertype NoteProperty -name "Feature Key" -Value $splitArr[0]
        $featureObject | add-member -membertype NoteProperty -name "Description" -Value "$desc"
        $featureObject | add-member -membertype NoteProperty -name "Type(PREVIEW,BETA,RC)" -Value $splitArr[1]
    
        if($splitArr.Count -eq 3){
            $featureObject | add-member -membertype NoteProperty -name "Minor key" -Value $splitArr[2]
        }
        else{
            $featureObject | add-member -membertype NoteProperty -name "Minor key" -Value ""
        }

        $csvContents += $featureObject
    }
    else{
        Write-Host "Error, File is missing the correct number of expected segments: " $outfile
        $returnStatus = -1
    }
}

# export
$ReportFile = [io.path]::combine($OutPutDir, "ringtail-static-feature-data.csv")
Write-Host "Report being generated : " $ReportFile
$csvContents | Export-csv $ReportFile -notypeinformation

return $returnStatus
