$webClient = New-Object System.Net.WebClient
$json = $webClient.DownloadString("https://api.bintray.com/packages/southpolenator/WinDbgCs_dumps/NativeDumpTest/files");
$files = ConvertFrom-Json $json
foreach ($file in $files)
{
    $url = "https://dl.bintray.com/southpolenator/WinDbgCs_dumps/" + $file.path
    $webClient.DownloadFile($url, $file.name);
}
