@cd IronMath
@rmdir /S /Q Debug Release bin obj
@del /S /Q *.ncb *.opt *.suo *.exe *.aps *.pdb *.dll

@cd ..\IronPython
@rmdir /S /Q Debug Release bin obj
@del /S /Q *.ncb *.opt *.suo *.exe *.aps *.pdb *.dll

@cd ..\IronPythonConsole
@rmdir /S /Q Debug Release bin obj
@del /S /Q *.ncb *.opt *.suo *.exe *.aps *.pdb *.dll

@cd ..\IronPythonTest
@rmdir /S /Q Debug Release bin obj
@del /S /Q *.ncb *.opt *.suo *.exe *.aps *.pdb *.dll

@cd ..\..
@rmdir /S /Q Debug Release bin obj
@del /S /Q *.ncb *.opt *.suo *.exe *.aps *.pdb *.dll

@cd Src
@pause

