; Chess Engine Installer Script
SetCompressor lzma

; Defines
!define PRODUCT_NAME "Kio Chess"
!define PRODUCT_VERSION "1.0.0"
!define PRODUCT_PUBLISHER "Kio Chess"
!define PRODUCT_WEB_SITE "https://github.com/KostyaBaklan/Kio-Chess"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\Application.exe"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"

; MUI Settings
!include "MUI2.nsh"
!define MUI_ABORTWARNING
!define MUI_ICON "..\\KioChess\\Application\\chess_icon_9bb_icon.ico"
!define MUI_UNICON "..\\KioChess\\Application\\chess_icon_9bb_icon.ico"

; Welcome page
!insertmacro MUI_PAGE_WELCOME
; License page
!insertmacro MUI_PAGE_LICENSE "..\LICENSE"
; Directory page
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
!define MUI_FINISHPAGE_RUN "$INSTDIR\Application.exe"
!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES

; Language files
!insertmacro MUI_LANGUAGE "English"

; Product Info
Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "KioChessSetup.exe"
InstallDir "$PROGRAMFILES64\Kio Chess"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show

Section "InstallDb" SEC_DB
    SetOutPath "C:\Dev\ChessDB"
    File "C:\Dev\ChessDB\chessApp.db"
SectionEnd

Section "CopyFiles" SEC_FILES
    SetOutPath "$INSTDIR"
    File /r /x *.pdb "..\KioChess\Application\bin\Release\net9.0-windows7.0\*.*"
SectionEnd

Section "CreateShortcuts" SEC_SHORTCUTS
    CreateDirectory "$SMPROGRAMS\Kio Chess"
    CreateShortCut "$SMPROGRAMS\Kio Chess\Kio Chess.lnk" "$INSTDIR\Application.exe"
    CreateShortCut "$DESKTOP\Kio Chess.lnk" "$INSTDIR\Application.exe"
SectionEnd

Section "Registry" SEC_REGISTRY
    WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\Application.exe"
    WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "DisplayName" "$^(Name)"
    WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninstall.exe"
    WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\Application.exe"
    WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
    WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
    WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
SectionEnd

Section "Uninstaller" SEC_UNINSTALLER
    WriteUninstaller "$INSTDIR\uninstall.exe"
SectionEnd

Section Uninstall
    ; Remove all Kio Chess shortcuts from Start Menu and Desktop
    Delete "$SMPROGRAMS\Kio Chess\*.lnk"
    Delete "$DESKTOP\Kio Chess.lnk"
    Delete "$DESKTOP\Kio_Chess.lnk"
    Delete "$DESKTOP\KioChess.lnk"
    RMDir "$SMPROGRAMS\Kio Chess"
    
    ; Remove files
    RMDir /r "$INSTDIR"
    Delete "C:\Dev\ChessDB\chessApp.db"
    
    ; Remove registry entries
    DeleteRegKey HKLM "${PRODUCT_UNINST_KEY}"
    DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
SectionEnd