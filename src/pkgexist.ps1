$pkgname=$args[0]
$pkgver=$args[1]

If (-Not (Test-Path .\Packages\$pkgname)) {
	echo true
	return
}

$currver=(Get-Content .\Packages\$pkgname\package.json | ConvertFrom-Json).version
If ($currver -ne $pkgver) {
	echo true
}
else {
	echo false
}