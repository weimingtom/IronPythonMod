@rmdir /S /Q Debug Release bin obj
@del /S /Q *.ncb *.opt *.exe *.aps *.pdb *.dll *.user
@del /S /A /A:H *.suo

@cd Src
@pause

