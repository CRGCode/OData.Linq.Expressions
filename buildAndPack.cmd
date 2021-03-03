@if "%1"=="" goto usage

:build
dotnet build OData.Linq.Expressions.sln -c Release -p:VersionPrefix=%1
@goto pack

:pack
call c:\Nuget\nuget pack OData.Linq.Expressions.nuspec -Symbols -Version %1 -OutputDirectory build\packages -SymbolPackageFormat snupkg
@goto end

:usage
 echo Usage: buildAndPack version-number
 @goto end

:end