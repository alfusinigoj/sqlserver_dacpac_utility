
### SqlServer Dacpac Utility (dacpac-cli.exe)
Command line helper utility for executing SQL Server Data-Tier Application Framework package (dacpac). For more info, refer [here](https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.dac?view=sql-dacfx-150)

### Release

- [![Prod Release](https://github.com/alfusinigoj/sqlserver_dacpac_utility/actions/workflows/pipeline_release.yml/badge.svg)](https://github.com/alfusinigoj/sqlserver_dacpac_utility/actions/workflows/pipeline_release.yml)

- Download latest `dacpac-cli.exe` [here](https://github.com/alfusinigoj/sqlserver_dacpac_utility/releases)

### Usage:
  `dacpac-cli.exe [options]`

| Options | Is Required | Description | Can pull from Environment Variable |
| --- | --- | --- |-- |
| -c, --command <command> | Yes | Dacpac command (script, deploy and extract) | No |
| -f, --package <package> | Yes | Dacpac package full path or relative to the this exe including dacpac filename. | Yes |
| -n, --connection <connection> | Yes |  Connection string of the target database. | Yes |
| -p, --parameters  | No | Array of parameters (`\|` seperated, to do `@@token_name@@` token replacement in `post deploy scripts`. E.g. To replace token `@@DATABASE@@` and `@@SERVER@@` in a script, your command argument should be -p [SERVER=env:`<env variable>`\|DATABASE=database1] | No |
| -a, --arguments <arguments>  | No |  Array of arguments for commands; For `script` and `deploy` use [DacDeployOptions](https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.dac.dacdeployoptions?view=sql-dacfx-150); For `extract` use [DacExtractOptions](https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.dac.dacextractoptions?view=sql-dacfx-150); E.g -a [BlockOnPossibleDataLoss=true,IncludeTransactionalScripts=false]  | No |
| --version   | No |  Show assembly version information | No |
| -?, -h, --help   | No |  Show help and usage information | No |

### Note
> Eligible arguments can be passed as environment variable, as env:<variable_name>

### Sample Commands
```
dacpac-cli.exe -c deploy -f MyDB.dacpac -n "Data Source=server1;Initial Catalog=db1;Persist Security Info=True;User ID=user1;Password=password1" -a [BlockOnPossibleDataLoss=false]
```

```
dacpac-cli.exe -c deploy -f MyDB.dacpac -n "env:ConnectionString" -a [BlockOnPossibleDataLoss=false]
```
  
### Compile locally
- Download the source code
- Execute the command `.\build.cmd dp`, which will produce the executable under `publish-artifacts`

### Any issues or concerns
- Please raise issues [here](https://github.com/alfusinigoj/sqlserver_dacpac_utility/issues)
