
### PivotalServices.DacpacDeploy.Utility (DacpacDeployUtil.exe)
Command line helper utility for executing SQL Server Data-Tier Application Framework package (dacpac). For more info, refer [here](https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.dac?view=sql-dacfx-150)

### Release
Download latest `DacpacDeployUtil.exe` [here]()

### Usage:
  `DacpacDeployUtil.exe [options]`

| Options | Is Required | Description | From Environment Variable |
| --- | --- | --- |-- |
| -c, --command <command> | Yes | Dacpac command (script, deploy and extract) | No |
| -f, --package <package> | Yes | Dacpac package full path or relative to the this exe including dacpac filename. | Yes |
| -n, --connection <connection> | Yes |  Connection string of the target database. | Yes |
| -p, --parameters  | No | Array of parameters (`\|` seperated, to do `@@token_name@@` token replacement in dml/sqlagentjob scripts. E.g. To replace token `@@SERVER@@` and `@@DATABASE@@`for a sqlagent job your command argument should be -p [SERVER=env:SERVER_NM\|DATABASE=database1] | No |
| -a, --arguments <arguments>  | No |  Array of arguments for commands; For `script` and `deploy` use [DacDeployOptions](https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.dac.dacdeployoptions?view=sql-dacfx-150); For `extract` use [DacExtractOptions](https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.dac.dacextractoptions?view=sql-dacfx-150); E.g -a [BlockOnPossibleDataLoss=true,IncludeTransactionalScripts=false]  | No |
| --version   | No |  Show assembly version information | No |
| -?, -h, --help   | No |  Show help and usage information | No |

### Note
> 1. Eligible arguments can be passed as environment variable, as env:<variable_name>
> 2. Any object with tokens will always be redeployed regardless of its change status (modified or not)

### Sample Commands
```
DacpacDeployUtil.exe -c deploy -f MyDB.dacpac -n "Data Source=server1;Initial Catalog=db1;Persist Security Info=True;User ID=user1;Password=password1" -a [BlockOnPossibleDataLoss=false] -p [SERVER=server2] 
```

```
DacpacDeployUtil.exe -c deploy -f MyDB.dacpac -n "env:ConnectionString" -a [BlockOnPossibleDataLoss=false] -p [SERVER=server2]
```

- **Hope this help you! For any issues, please raise issues [here](https://github.com/alfusinigoj/sqlserver_dacpac_utility/issues)**


