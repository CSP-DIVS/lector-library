# using swagger
run `dotnet build`
it will build documentation xml file inside build debug folder

setup and start DB.

run `dotnet run`
the swagger will read that xml and creat swagger ui, and go the deplyeport/swagger
for example if its `http://localhost:5192` then go to `http://localhost:5192/swagger`

# using docfx

cd to `backend/Csp.Api`

run `dotnet build`

## install docfx in global and add it to path

run `dotnet tool install -g docfx` and make sure its added to path. `~/.dotnet/tools` on unix and `%USERPROFILE%\.dotnet\tools` on windows.

if docx.json not exist in Csp.Api run `docfx init -q` to generate that file.

run `docfx metadata` // extract the yaml generated inside build folder creat yaml metadata file

run `docfx /path/of/docfx.json --serve`

go to ` http://localhost:8080`