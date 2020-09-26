#Region Startup
	#AutoIt3Wrapper_Icon=icon.ico
	#AutoIt3Wrapper_Res_Description=Winuntu
	#AutoIt3Wrapper_Res_Fileversion=5.6.0.0
	#AutoIt3Wrapper_Res_ProductName=Winuntu
	#AutoIt3Wrapper_Res_ProductVersion=5.6.0.0
	#AutoIt3Wrapper_Res_LegalCopyright=© Bustos Thibault - 2019
	Global $version = "5.6.0.0"
	Global $copyright = "© Bustos Thibault - 2019"
	#NoTrayIcon
	If @OSVersion <> "WIN_10" Then
		MsgBox(0 + 16, "Erreur !", "Winuntu est fait pour Windows 10.")
		Exit
	EndIf
	If IsAdmin() Then
		Global $titlewinuntu = "(Administrateur) Winuntu"
	Else
		Global $titlewinuntu = "Winuntu"
	EndIf
	$fondecran = IniRead(@AppDataDir & "\Winuntu.ini", "Wallpaper", "Pictures", "")
	Global $numbertab = 0
	Global $numberalltab = 0
	Global $gui_main_tab_onglet_addcmd = ""
	Global $gui_main_tab_onglet_addpowershell = ""
#EndRegion Startup
#Region Gui_Main
	$gui_main = GUICreate($titlewinuntu & " - (0)", 800, 525, -1, -1, 13565952)
	GUISetFont(12)
	$gui_main_menu_instances = GUICtrlCreateMenu("Instances")
	$gui_main_menu_instances_cmd = GUICtrlCreateMenuItem("Ajouter une instance Invite de commandes	F1", $gui_main_menu_instances)
	$gui_main_menu_instances_powershell = GUICtrlCreateMenuItem("Ajouter une instance Windows PowerShell	F2", $gui_main_menu_instances)
	GUICtrlCreateMenuItem("", $gui_main_menu_instances)
	$gui_main_menu_instances_path = GUICtrlCreateMenuItem("Ajouter un dossier de cette instance", $gui_main_menu_instances)
	$gui_main_menu_instances_file = GUICtrlCreateMenuItem("Ajouter un fichier à cette instance", $gui_main_menu_instances)
	GUICtrlCreateMenuItem("", $gui_main_menu_instances)
	$gui_main_menu_instances_close = GUICtrlCreateMenuItem("Fermer cette instance	F3", $gui_main_menu_instances)
	$gui_main_menu_mode = GUICtrlCreateMenu("Mode")
	$gui_main_menu_mode_admin = GUICtrlCreateMenuItem("Relancer Winuntu en mode administrateur", $gui_main_menu_mode)
	If IsAdmin() Then GUICtrlSetState(-1, 128)
	$gui_main_menu_wall = GUICtrlCreateMenu("Fond d'écran")
	$gui_main_menu_wall_set = GUICtrlCreateMenuItem("Changer le fond d'écran", $gui_main_menu_wall)
	GUICtrlCreateMenuItem("", $gui_main_menu_wall)
	$gui_main_menu_wall_rem = GUICtrlCreateMenuItem("Retirer le fond d'écran", $gui_main_menu_wall)
	$gui_main_menu_shortcut = GUICtrlCreateMenu("Raccourci")
	$gui_main_menu_shortcut_task = GUICtrlCreateMenuItem("Ouvrir le gestionnaire des tâches", $gui_main_menu_shortcut)
	$gui_main_menu_shortcut_reg = GUICtrlCreateMenuItem("Ouvrir le registre Windows", $gui_main_menu_shortcut)
	GUICtrlCreateMenuItem("", $gui_main_menu_shortcut)
	For $i = 1 To 9 Step +1
		$a = IniRead(@AppDataDir & "\Winuntu.ini", "Shortcut", $i, "")
		Assign("Gui_Main_Menu_ShortCut_" & $i & "_Add", "1")
		Assign("Gui_Main_Menu_ShortCut_" & $i & "_Open", "1")
		Assign("Gui_Main_Menu_ShortCut_" & $i & "_Sep", "1")
		Assign("Gui_Main_Menu_ShortCut_" & $i & "_Rem", "1")
		If $a = "" Then
			Assign("Gui_Main_Menu_ShortCut_" & $i, GUICtrlCreateMenu($i & ": ...", $gui_main_menu_shortcut))
			Assign("Gui_Main_Menu_ShortCut_" & $i & "_Add", GUICtrlCreateMenuItem("Créer ce raccourci", Eval("Gui_Main_Menu_ShortCut_" & $i)))
		Else
			Assign("Gui_Main_Menu_ShortCut_" & $i, GUICtrlCreateMenu($i & ": " & $a, $gui_main_menu_shortcut))
			Assign("Gui_Main_Menu_ShortCut_" & $i & "_Open", GUICtrlCreateMenuItem("Ouvrir ce raccourci", Eval("Gui_Main_Menu_ShortCut_" & $i)))
			Assign("Gui_Main_Menu_ShortCut_" & $i & "_Sep", GUICtrlCreateMenuItem("", Eval("Gui_Main_Menu_ShortCut_" & $i)))
			Assign("Gui_Main_Menu_ShortCut_" & $i & "_Rem", GUICtrlCreateMenuItem("Supprimer ce raccourci", Eval("Gui_Main_Menu_ShortCut_" & $i)))
		EndIf
	Next
	$gui_main_menu_help = GUICtrlCreateMenu("?")
	$gui_main_menu_apropos = GUICtrlCreateMenuItem("À propos...", $gui_main_menu_help)
	$gui_main_tab_onglet = GUICtrlCreateTab(0, 0, 805, 30)
	GUICtrlSetResizing(-1, 4 + 2 + 32 + 512)
	GUICtrlCreateTabItem("")
	$gui_instances = GUICreate("", 800, 525, 0, 30, 1073741824, -1, $gui_main)
	GUISetBkColor(0)
	$gui_instances_wallpaper = GUICtrlCreatePic($fondecran, 0, 0, 800, 525, 128)
#EndRegion Gui_Main
#Region Brain
	GUISetState(@SW_SHOW, $gui_instances)
	GUISetState(@SW_SHOW, $gui_main)
	__addtab("cmd.exe", " /k prompt [$D$S$T]$S%username%:~$$ && echo; _       ___                   __        && echo;^| ^|     / (_)___  __  ______  / /___  __ && echo;^| ^| /^| / / / __ \/ / / / __ \/ __/ / / / version && echo;^| ^|/ ^|/ / / / / / /_/ / / / / /_/ /_/ / " & $version & " && echo;^|__/^|__/_/_/ /_/\__,_/_/ /_/\__/\__,_/ && echo;" & $copyright, "Invite de commandes")
	__addtab("powershell.exe", " -NoExit -Command function prompt {\""[$(get-date)] $env:UserName~:$ \""} ""echo ' _       ___                   __'"" ""'| |     / (_)___  __  ______  / /___  __'"" ""'| | /| / / / __ \/ / / / __ \/ __/ / / / version'"" ""'| |/ |/ / / / / / /_/ / / / / /_/ /_/ / " & $version & "'"" ""'|__/|__/_/_/ /_/\__,_/_/ /_/\__/\__,_/'"" ""'" & $copyright & "'"" ""''""", "Windows PowerShell")
	GUIRegisterMsg(5, "__ReloadGui")
	While 1
		If NOT ProcessExists(Eval("Gui_Main_Instances_N" & GUICtrlRead($gui_main_tab_onglet) + 1 & "_Pid")) AND GUICtrlRead($gui_main_tab_onglet) <> $numbertab AND GUICtrlRead($gui_main_tab_onglet) <> $numbertab + 1 Then
			__removetab(GUICtrlRead($gui_main_tab_onglet) + 1)
		EndIf
		If WinActive(Eval("Gui_Main_Instances_N" & GUICtrlRead($gui_main_tab_onglet) + 1 & "_Hwnd")) Then
			WinSetOnTop($gui_main, "", 1)
			WinSetOnTop($gui_main, "", 0)
		EndIf
		$a = DllCall("user32.dll", "short", "GetAsyncKeyState", "int", "0x70")
		If $a[0] = -32768 Then
			__addtab("cmd.exe", " /k prompt [$D$S$T]$S%username%:~$$ && echo; _       ___                   __        && echo;^| ^|     / (_)___  __  ______  / /___  __ && echo;^| ^| /^| / / / __ \/ / / / __ \/ __/ / / / version && echo;^| ^|/ ^|/ / / / / / /_/ / / / / /_/ /_/ / " & $version & " && echo;^|__/^|__/_/_/ /_/\__,_/_/ /_/\__/\__,_/ && echo;" & $copyright, "Invite de commandes")
			Sleep(200)
		EndIf
		$a = DllCall("user32.dll", "short", "GetAsyncKeyState", "int", "0x71")
		If $a[0] = -32768 Then
			__addtab("powershell.exe", " -NoExit -Command function prompt {\""[$(get-date)] $env:UserName~:$ \""} ""echo ' _       ___                   __'"" ""'| |     / (_)___  __  ______  / /___  __'"" ""'| | /| / / / __ \/ / / / __ \/ __/ / / / version'"" ""'| |/ |/ / / / / / /_/ / / / / /_/ /_/ / " & $version & "'"" ""'|__/|__/_/_/ /_/\__,_/_/ /_/\__/\__,_/'"" ""'" & $copyright & "'"" ""''""", "Windows PowerShell")
			Sleep(200)
		EndIf
		$a = DllCall("user32.dll", "short", "GetAsyncKeyState", "int", "0x72")
		If $a[0] = -32768 Then
			__removetab(GUICtrlRead($gui_main_tab_onglet) + 1)
			Sleep(200)
		EndIf
		$guimsg = GUIGetMsg()
		Switch $guimsg
			Case -3
				$a = MsgBox(4 + 256 + 32, "Quitter ?", "Êtes-vous sûr de vouloir quitter ?")
				If $a = 6 Then
					__exit()
				EndIf
			Case -12, -6
				GUICtrlSetImage($gui_instances_wallpaper, $fondecran)
			Case $gui_main_tab_onglet
				If GUICtrlRead($gui_main_tab_onglet) = $numbertab Then
					__addtab("cmd.exe", " /k prompt [$D$S$T]$S%username%:~$$ && echo; _       ___                   __        && echo;^| ^|     / (_)___  __  ______  / /___  __ && echo;^| ^| /^| / / / __ \/ / / / __ \/ __/ / / / version && echo;^| ^|/ ^|/ / / / / / /_/ / / / / /_/ /_/ / " & $version & " && echo;^|__/^|__/_/_/ /_/\__,_/_/ /_/\__/\__,_/ && echo;" & $copyright, "Invite de commandes")
					ContinueLoop
				EndIf
				If GUICtrlRead($gui_main_tab_onglet) = $numbertab + 1 Then
					__addtab("powershell.exe", " -NoExit -Command function prompt {\""[$(get-date)] $env:UserName~:$ \""} ""echo ' _       ___                   __'"" ""'| |     / (_)___  __  ______  / /___  __'"" ""'| | /| / / / __ \/ / / / __ \/ __/ / / / version'"" ""'| |/ |/ / / / / / /_/ / / / / /_/ /_/ / " & $version & "'"" ""'|__/|__/_/_/ /_/\__,_/_/ /_/\__/\__,_/'"" ""'" & $copyright & "'"" ""''""", "Windows PowerShell")
					ContinueLoop
				EndIf
				__reloadgui()
			Case $gui_main_menu_instances_cmd
				__addtab("cmd.exe", " /k prompt [$D$S$T]$S%username%:~$$ && echo; _       ___                   __        && echo;^| ^|     / (_)___  __  ______  / /___  __ && echo;^| ^| /^| / / / __ \/ / / / __ \/ __/ / / / version && echo;^| ^|/ ^|/ / / / / / /_/ / / / / /_/ /_/ / " & $version & " && echo;^|__/^|__/_/_/ /_/\__,_/_/ /_/\__/\__,_/ && echo;" & $copyright, "Invite de commandes")
			Case $gui_main_menu_instances_powershell
				__addtab("powershell.exe", " -NoExit -Command function prompt {\""[$(get-date)] $env:UserName~:$ \""} ""echo ' _       ___                   __'"" ""'| |     / (_)___  __  ______  / /___  __'"" ""'| | /| / / / __ \/ / / / __ \/ __/ / / / version'"" ""'| |/ |/ / / / / / /_/ / / / / /_/ /_/ / " & $version & "'"" ""'|__/|__/_/_/ /_/\__,_/_/ /_/\__/\__,_/'"" ""'" & $copyright & "'"" ""''""", "Windows PowerShell")
			Case $gui_main_menu_instances_path
				$a = FileSelectFolder("Choisissez un dossier à ajouter à cette instance....", @ScriptDir)
				If $a <> "" Then
					WinActivate(Eval("Gui_Main_Instances_N" & GUICtrlRead($gui_main_tab_onglet) + 1 & "_Hwnd"))
					Send('"' & $a & '"', 1)
				EndIf
			Case $gui_main_menu_instances_file
				$a = FileOpenDialog("Choisissez un fichier à ajouter à cette instance...", @ScriptDir, "Tous les fichiers (*.*)")
				If $a <> "" Then
					WinActivate(Eval("Gui_Main_Instances_N" & GUICtrlRead($gui_main_tab_onglet) + 1 & "_Hwnd"))
					Send('"' & $a & '"', 1)
				EndIf
			Case $gui_main_menu_instances_close
				__removetab(GUICtrlRead($gui_main_tab_onglet) + 1)
			Case $gui_main_menu_mode_admin
				ShellExecute(@ScriptFullPath, "", "", "runas")
				__exit()
			Case $gui_main_menu_wall_set
				$a = FileOpenDialog("Choisissez un fond d'écran...", @ScriptDir, "Images (*.jpg)", 1 + 2)
				If $a <> "" Then
					$fondecran = $a
					IniWrite(@AppDataDir & "\Winuntu.ini", "Wallpaper", "Pictures", $fondecran)
					GUICtrlSetImage($gui_instances_wallpaper, $fondecran)
					GUICtrlSetPos($gui_instances_wallpaper, 0, 0, 0, 0)
					__reloadgui()
				EndIf
			Case $gui_main_menu_wall_rem
				$fondecran = ""
				IniWrite(@AppDataDir & "\Winuntu.ini", "Wallpaper", "Pictures", $fondecran)
				GUICtrlSetImage($gui_instances_wallpaper, "")
				__reloadgui()
			Case $gui_main_menu_shortcut_task
				ShellExecute("taskmgr")
			Case $gui_main_menu_shortcut_reg
				ShellExecute("regedit")
			Case $gui_main_menu_apropos
				MsgBox(0 + 64, "À propos...", "Winuntu" & @CRLF & "" & @CRLF & "Auteur : Bustos Thibault" & @CRLF & "Version : " & $version & @CRLF & "Copyright : " & $copyright & @CRLF & "Langage : AutoIt 3" & @CRLF & "" & @CRLF & "Me joindre : thibault.bustos1234@gmail.com")
		EndSwitch
		For $i = 1 To 9 Step +1
			Switch $guimsg
				Case Eval("Gui_Main_Menu_ShortCut_" & $i & "_Add")
					$a = FileOpenDialog("Choisissez une application pour créer ce raccourci...", @ScriptDir, "Tous les fichiers (*.*)", 1 + 2)
					If $a <> "" Then
						GUICtrlSetData(Eval("Gui_Main_Menu_ShortCut_" & $i), $i & ": " & $a)
						GUICtrlDelete(Eval("Gui_Main_Menu_ShortCut_" & $i & "_Add"))
						Assign("Gui_Main_Menu_ShortCut_" & $i & "_Add", "1")
						Assign("Gui_Main_Menu_ShortCut_" & $i & "_Open", GUICtrlCreateMenuItem("Ouvrir ce raccourci", Eval("Gui_Main_Menu_ShortCut_" & $i)))
						Assign("Gui_Main_Menu_ShortCut_" & $i & "_Sep", GUICtrlCreateMenuItem("", Eval("Gui_Main_Menu_ShortCut_" & $i)))
						Assign("Gui_Main_Menu_ShortCut_" & $i & "_Rem", GUICtrlCreateMenuItem("Supprimer ce raccourci", Eval("Gui_Main_Menu_ShortCut_" & $i)))
						IniWrite(@AppDataDir & "\Winuntu.ini", "Shortcut", $i, $a)
					EndIf
				Case Eval("Gui_Main_Menu_ShortCut_" & $i & "_Open")
					ShellExecute(IniRead(@AppDataDir & "\Winuntu.ini", "Shortcut", $i, ""))
				Case Eval("Gui_Main_Menu_ShortCut_" & $i & "_Rem")
					GUICtrlSetData(Eval("Gui_Main_Menu_ShortCut_" & $i), $i & ": ...")
					GUICtrlDelete(Eval("Gui_Main_Menu_ShortCut_" & $i & "_Open"))
					Assign("Gui_Main_Menu_ShortCut_" & $i & "_Open", "1")
					GUICtrlDelete(Eval("Gui_Main_Menu_ShortCut_" & $i & "_Sep"))
					Assign("Gui_Main_Menu_ShortCut_" & $i & "_Sep", "1")
					GUICtrlDelete(Eval("Gui_Main_Menu_ShortCut_" & $i & "_Rem"))
					Assign("Gui_Main_Menu_ShortCut_" & $i & "_Rem", "1")
					Assign("Gui_Main_Menu_ShortCut_" & $i & "_Add", GUICtrlCreateMenuItem("Créer ce raccourci", Eval("Gui_Main_Menu_ShortCut_" & $i)))
					IniWrite(@AppDataDir & "\Winuntu.ini", "Shortcut", $i, "")
			EndSwitch
		Next
	WEnd
#EndRegion Brain
#Region Func

	Func __exit()
		GUIDelete($gui_instances)
		GUIDelete($gui_main)
		For $i = 1 To $numbertab Step +1
			Do
				ProcessClose(Eval("Gui_Main_Instances_N" & $i & "_Pid"))
			Until NOT ProcessExists(Eval("Gui_Main_Instances_N" & $i & "_Pid"))
		Next
		Exit
	EndFunc

	Func __reloadgui()
		$a = WinGetPos($gui_main)
		If $a[2] > @DesktopWidth Then WinMove($gui_main, "", $a[0], $a[1], @DesktopWidth, $a[3])
		$a = WinGetPos($gui_main)
		If $a[3] > @DesktopHeight Then WinMove($gui_main, "", $a[0], $a[1], $a[2], @DesktopHeight)
		If Eval("Gui_Main_Instances_N" & GUICtrlRead($gui_main_tab_onglet) + 1 & "_Hwnd") = "" Then Return 
		$a = WinGetClientSize($gui_main)
		WinMove($gui_instances, "", 0, 30, $a[0], $a[1] - 30)
		If $fondecran = "" Then
			WinSetTrans(Eval("Gui_Main_Instances_N" & GUICtrlRead($gui_main_tab_onglet) + 1 & "_Hwnd"), "", 255)
		Else
			WinSetTrans(Eval("Gui_Main_Instances_N" & GUICtrlRead($gui_main_tab_onglet) + 1 & "_Hwnd"), "", 200)
		EndIf
		$a = WinGetClientSize($gui_instances)
		WinMove(Eval("Gui_Main_Instances_N" & GUICtrlRead($gui_main_tab_onglet) + 1 & "_Hwnd"), "", -8, -31, $a[0] + 16, $a[1] + 39)
		For $i = $numbertab To 1 Step -1
			If $i <> GUICtrlRead($gui_main_tab_onglet) + 1 Then
				WinSetTrans(Eval("Gui_Main_Instances_N" & $i & "_Hwnd"), "", 0)
			EndIf
		Next
		GUICtrlSetPos($gui_instances_wallpaper, 0, 0, $a[0], $a[1])
	EndFunc

	Func __removetab($number)
		GUICtrlDelete(Eval("Gui_Main_Tab_Onglet_N" & $number))
		Do
			ProcessClose(Eval("Gui_Main_Instances_N" & $number & "_Pid"))
		Until NOT ProcessExists(Eval("Gui_Main_Instances_N" & $number & "_Pid"))
		For $i = $number To $numbertab Step +1
			Assign("Gui_Main_Instances_N" & $i & "_Pid", Eval("Gui_Main_Instances_N" & $i + 1 & "_Pid"), 2)
			Assign("Gui_Main_Instances_N" & $i & "_Hwnd", Eval("Gui_Main_Instances_N" & $i + 1 & "_Hwnd"), 2)
			Assign("Gui_Main_Tab_Onglet_N" & $i, Eval("Gui_Main_Tab_Onglet_N" & $i + 1), 2)
		Next
		$numbertab -= 1
		If $numbertab = 0 Then __exit()
		WinSetTitle($gui_main, "", $titlewinuntu & " - (" & $numbertab & ")")
		GUICtrlSetState(Eval("Gui_Main_Tab_Onglet_N" & GUICtrlRead($gui_main_tab_onglet)), 16)
		__reloadgui()
	EndFunc

	Func __addtab($execute, $params, $tabname)
		$numbertab += 1
		$numberalltab += 1
		GUICtrlDelete($gui_main_tab_onglet_addcmd)
		GUICtrlDelete($gui_main_tab_onglet_addpowershell)
		Assign("Gui_Main_Instances_N" & $numbertab & "_Pid", ShellExecute($execute, $params, @ScriptDir, "open"), 2)
		Do
		Until ProcessExists(Eval("Gui_Main_Instances_N" & $numbertab & "_Pid"))
		Do
			$a = WinList()
			For $i = 1 To $a[0][0]
				If WinGetProcess($a[$i][1]) = Eval("Gui_Main_Instances_N" & $numbertab & "_Pid") Then
					Assign("Gui_Main_Instances_N" & $numbertab & "_Hwnd", $a[$i][1], 2)
				EndIf
			Next
		Until Eval("Gui_Main_Instances_N" & $numbertab & "_Hwnd") <> ""
		Do
		Until WinExists(Eval("Gui_Main_Instances_N" & $numbertab & "_Hwnd"))
		DllCall("user32.dll", "hwnd", "SetParent", "hwnd", Eval("Gui_Main_Instances_N" & $numbertab & "_Hwnd"), "hwnd", $gui_instances)
		GUISwitch($gui_main)
		Assign("Gui_Main_Tab_Onglet_N" & $numbertab, GUICtrlCreateTabItem("(" & $numberalltab & ") " & $tabname), 2)
		GUICtrlSetImage(-1, $execute)
		$gui_main_tab_onglet_addcmd = GUICtrlCreateTabItem("+")
		GUICtrlSetImage(-1, "cmd.exe")
		$gui_main_tab_onglet_addpowershell = GUICtrlCreateTabItem("+")
		GUICtrlSetImage(-1, "powershell.exe")
		GUICtrlCreateTabItem("")
		GUICtrlSetState(Eval("Gui_Main_Tab_Onglet_N" & $numbertab), 16)
		WinSetTitle($gui_main, "", $titlewinuntu & " - (" & $numbertab & ")")
		__reloadgui()
	EndFunc

#EndRegion Func
