@{
    Path = @(
        './out/package/lib',
        './src/AnyPackage.format.ps1xml',
        './src/AnyPackage.psd1',
        './src/AnyPackage.psm1'
    )
    Destination = './out/AnyPackage'
    Recurse = $true
}
