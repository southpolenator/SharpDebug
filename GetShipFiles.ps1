function Get-DllDependencies([string]$assemblyPath)
{
    $dependencies = @();
    $assemblyPath = Resolve-Path $assemblyPath;
    $folder = Split-Path $assemblyPath;
    $assembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom($assemblyPath);
    foreach ($a in $assembly.GetReferencedAssemblies())
    {
        $dependency = Join-Path $folder ($a.Name+".dll");
        if (Test-Path $dependency)
        {
            $dependencies += $dependency;
        }
    }
    return $dependencies;
}

function Get-AllDllDependencies([string]$assemblyPath)
{
    $dependencies = Get-DllDependencies($assemblyPath) | sort -unique;
    $newDependencies = $dependencies;
    (Resolve-Path $assemblyPath).Path
    if ($dependencies.GetType() -eq [string])
    {
        $dependencies
    }
    else
    {
        while ($newDependencies.Length -gt 0)
        {
            $item = $newDependencies[0];
            $item
            $nd = Get-DllDependencies($item) | sort -unique | where { $dependencies -notcontains $_ };
            $dependencies = $dependencies + $nd;
            $newDependencies = $newDependencies + $nd;
            if ($newDependencies.Length -gt 1)
            {
                $newDependencies = $newDependencies[1..($newDependencies.Length-1)];
            }
            else
            {
                break;
            }
        }
    }
}

function Get-DllShipFiles($dll)
{
    if (Test-Path $dll)
    {
        $dll
    }
    $xml = [System.IO.Path]::ChangeExtension($dll, "xml");
    if (Test-Path $xml)
    {
        $xml
    }
    $pdb = [System.IO.Path]::ChangeExtension($dll, "pdb");
    if (Test-Path $pdb)
    {
        $pdb
    }
    $config = $dll + ".config";
    if (Test-Path $config)
    {
        $config
    }
}

function Get-ShipFiles($dlls)
{
    $dlls | % { Get-AllDllDependencies($_) } | Sort -Unique | % { Get-DllShipFiles($_) }
}

#Get-ShipFiles(@(
#    #"SharpDebug.CodeGen.App.exe",
#    "SharpDebug.CommonUserTypes.dll",
#    "SharpDebug.DwarfSymbolProvider.dll",
#    "SharpDebug.WinDbg.x64.dll",
#    "SharpDebug.WinDbg.x86.dll"));
