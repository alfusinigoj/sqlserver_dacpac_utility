
### DacpacUtil
Command line helper utility for executing SQL Server Data-Tier Application Framework package (dacpac). For more info, refer [here](https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.dac?view=sql-dacfx-150)

### Release
Download latest `DacpacUtil.exe` [here]()

### Usage:
  `DacpacUtil.exe [options]`

| Options | Is Requires | Description | From Environment Variable |
| --- | --- | --- |-- |
| -c, --command <command> | Yes | Dacpac command (script, deploy and extract) | No |
| -p, --package <package> | Yes | Dacpac package full path or relative to the this exe including dacpac filename. | Yes |
| -n, --connection <connection> | Yes |  Connection string of the target database. | Yes |
| -a, --arguments <arguments>  | No |  Array of arguments for commands; For `script` and `deploy` use [DacDeployOptions](https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.dac.dacdeployoptions?view=sql-dacfx-150); For `extract` use [DacExtractOptions](https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.dac.dacextractoptions?view=sql-dacfx-150); E.g -a [BlockOnPossibleDataLoss=true,IncludeTransactionalScripts=false]  | No |
| --version   | No |  Show assembly version information | No |
| -?, -h, --help   | No |  Show help and usage information | No |

> Note: Eligible arguments can be passed as environment variable, as env:<variable_name>

### Sample Command
```
DacpacUtil.exe -c deploy -p XYZ.dacpac -n "Data Source=server1;Initial Catalog=db1;Persist Security Info=True;User ID=user1;Password=somepassword" -a [BlockOnPossibleDataLoss=false]
```

```
DacpacUtil.exe -c deploy -p "c:\XYZ.dacpac" -n "env:ConnectionString" -a [BlockOnPossibleDataLoss=false]
```

### Developer Notes
- Compile the application using the below commands, from the path where solution file is.
    - `build` - for compiling and running tests
    - `build p` - for publishing the application in `_publish` folder'
    - `build ci` - for integration build in ci bamboo pipeline
  - For more details, refer to `default.ps1`
  - To modify the assembly version based on releases, you can modify `build.properties` file with `major.minor.patch` format