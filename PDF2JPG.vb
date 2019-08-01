Imports System.Drawing
Imports System.IO
Imports System.Text.RegularExpressions
Imports GhostscriptSharp

Public Class PDF2JPG
    ' PDF2JPG.exe -- Convert a PDF file into JPG(s)
    ' Needs GhostscriptSharp to wrap the gsdll32.dll so it's a 32bit program.

    ' Read the number of pages of the PDF file
    Shared Function GetNumPages(ByVal path As String) As Integer
        Using reader As StreamReader = New StreamReader(path)
            Dim regex As New Regex("/Type\s*/Page[^s]")
            Dim matches = regex.Matches(reader.ReadToEnd())
            Return matches.Count
        End Using
    End Function

    ' Something went wrong? Please terminate the programm:
    Shared Sub Wrong(ByVal prog As String)
        Console.WriteLine("Wrong entry. Please use:" + vbNewLine +
                     "'" + prog + " -pdf <path to pdf> -jpg <path to jpg> -dpi <number> -a4|-letter'")
        End
    End Sub
    Shared Sub Main()
        Dim cliArgs() As String = Environment.GetCommandLineArgs()
        Dim GsInputPath As String = String.Empty
        Dim GsOutputPath As String = String.Empty
        Dim GsDpi As Integer
        Dim papersize As String

        If cliArgs.Count() = 8 Then
            ' Index  Discription:
            ' 0      Full path of exe
            ' 1      First switch: -pdf
            ' 2      First value 
            ' 3      Second switch: -jpg
            ' 4      Second value 
            ' 5      Third switch: -dpi
            ' 6      Third value
            ' 7      Fourth switch: -a4 or -letter
            For i As Integer = 1 To 7 Step 2
                If cliArgs(i) = "-pdf" Then
                    GsInputPath = cliArgs(i + 1)
                ElseIf cliArgs(i) = "-jpg" Then
                    GsOutputPath = cliArgs(i + 1)
                    ' We need the page number to output single JPG files and use %d for the number
                    GsOutputPath = Left(GsOutputPath, InStrRev(GsOutputPath, ".") - 1) & "_%d.jpg"
                ElseIf cliArgs(i) = "-dpi" Then
                    GsDpi = Convert.ToInt32(cliArgs(i + 1))
                ElseIf cliArgs(i) = "-a4" Or cliArgs(i) = "-letter" Then
                    papersize = cliArgs(i)
                Else
                    Wrong(cliArgs(0))
                End If
            Next
        Else
            Wrong(cliArgs(0))
        End If

        Dim GsSettings As New GhostscriptSettings
        Dim NumPages As Integer
        NumPages = GetNumPages(GsInputPath)

        With GsSettings
            .Device = Settings.GhostscriptDevices.jpeg
            .Page = New Settings.GhostscriptPages With {.Start = 1, .[End] = NumPages, .AllPages = True} ' .Start = 1, .[End] = 1,
            ' Render dpi
            .Resolution = New Size With {.Height = GsDpi, .Width = GsDpi}
            If papersize = "-a4" Then
                .Size = New Settings.GhostscriptPageSize With {.Native = Settings.GhostscriptPageSizes.a4}
            End If
            If papersize = "-letter" Then
                .Size = New Settings.GhostscriptPageSize With {.Native = Settings.GhostscriptPageSizes.letter}
            End If
        End With

        GhostscriptWrapper.GenerateOutput(GsInputPath, GsOutputPath, GsSettings)
    End Sub
End Class
