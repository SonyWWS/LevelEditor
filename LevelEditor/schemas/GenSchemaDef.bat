@set domgen=..\..\..\wws_atf\DevTools\DomGen\bin\DomGen.exe
@IF NOT EXIST %domgen% ( set domgen=..\..\ATF\DevTools\DomGen\DomGen.exe)
%domgen% level_editor.xsd Schema.cs "gap" LevelEditor