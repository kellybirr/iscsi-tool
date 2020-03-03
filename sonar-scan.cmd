SET JAVA_HOME=C:\sonar-scanner\sonar-scanner-4.1.0.1829\jre
C:\sonar-scanner\SonarScanner.MSBuild.exe begin /k:"kellybirr_iscsi-tool" /o:"kellybirr" /d:sonar.login="ce8ece62f5df3d7e2510be180a0ebd403e61da7f"
MSBuild.exe "iScsiTool.sln" /t:Rebuild
C:\sonar-scanner\SonarScanner.MSBuild.exe end /d:sonar.login="ce8ece62f5df3d7e2510be180a0ebd403e61da7f"