Imports System.IO
Imports System.Windows.Forms
Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Net.Mail

Namespace TechTraining

Public Class ttAccess
    Inherits System.Web.UI.Page
    
    Public Shared Function InsertClassRecord(ByVal UserName As String, ByVal ClassTime As String, ByVal ClassDate As Date, _
    										ByVal Enrolled As Boolean, ByVal ClassName As String, _
    										ByVal WaitListed As Boolean, ByVal Instructor As String, _
                                      		ByVal DateCompleted As Date, ByVal Completed As Boolean, _
                                      		ByVal Walkin As Boolean, ByVal UID As String) As Object

        Dim connStr As String = ConfigurationManager.AppSettings.Get("Subsite_appSettings")
        Dim conn As New Data.OleDb.OleDbConnection(connStr)

        conn.Open()
        Dim sql As String = "INSERT INTO EnrollmentsTbl (" & _
        "[UserName],[SubmitTime],[ClassTime],[ClassDate],[Enrolled],[ClassName],[WaitListed]," & _
        "[Instructor],[DateCompleted],[Completed],[Walkin],[UID]) VALUES " & _
        "(@UserName, @SubmitTime, @ClassTime, @ClassDate, @Enrolled, @ClassName, @WaitListed, " & _
        "@Instructor, @DateCompleted, @Completed, @Walkin, @UID) "

        Dim comm As New Data.OleDb.OleDbCommand(sql, conn)
        comm.Parameters.AddWithValue("@UserName", UserName)
        comm.Parameters.AddWithValue("@SubmitTime", DateTime.Now.AddHours(3).ToString())
        comm.Parameters.AddWithValue("@ClassTime", ClassTime)
        comm.Parameters.AddWithValue("@ClassDate", ClassDate)
        comm.Parameters.AddWithValue("@Enrolled", Enrolled)
        comm.Parameters.AddWithValue("@ClassName", ClassName)
        comm.Parameters.AddWithValue("@WaitListed", WaitListed)
        comm.Parameters.AddWithValue("@Instructor", Instructor)
        comm.Parameters.AddWithValue("@DateCompleted", DateCompleted)
        comm.Parameters.AddWithValue("@Completed", Completed)
        comm.Parameters.AddWithValue("@Walkin", Walkin)
        comm.Parameters.AddWithValue("@UID", UID)


        Dim result As Integer = comm.ExecuteNonQuery()
        conn.Close()

        Return True

    End Function
    
    Public Shared Function SelectRecords() As Object
    	
    	Dim user_connetionString As String
        Dim user_connection As data.OleDb.OleDbConnection
        Dim oledbAdapter As Data.OleDb.OleDbDataAdapter
        Dim user_sql As String
        Dim ds As New Data.DataSet
    	Dim fileLineArray() As String = Nothing
    	Dim crosstab_dt As New Data.DataTable

        user_connetionString = ConfigurationManager.ConnectionStrings("AccessSubsite").ToString                
        user_sql = "TRANSFORM First(Format(ClassRecordsEnrollmentsQry.DateCompleted,'mm/dd/yyyy')) AS FirstOfDateCompleted " & _
            "SELECT ClassRecordsEnrollmentsQry.StudentID, ClassRecordsEnrollmentsQry.LastName, ClassRecordsEnrollmentsQry.FirstName, " & _
            "ClassRecordsEnrollmentsQry.Affiliation, Sum(ClassRecordsEnrollmentsQry.ALS) AS ALS " & _
            "FROM ClassRecordsEnrollmentsQry " & _
            "GROUP BY ClassRecordsEnrollmentsQry.StudentID, ClassRecordsEnrollmentsQry.LastName, ClassRecordsEnrollmentsQry.FirstName, " & _
            "ClassRecordsEnrollmentsQry.Affiliation " & _
            "ORDER BY ClassRecordsEnrollmentsQry.LastName " & _
            "PIVOT Left(ClassRecordsEnrollmentsQry.ClassName,InStr(ClassRecordsEnrollmentsQry.ClassName,' ('));"
                
        user_connection = New data.OleDb.OleDbConnection(user_connetionString)

    
            user_connection.Open()
            oledbAdapter = New Data.OleDb.OleDbDataAdapter(user_sql, user_connection)
            oledbAdapter.Fill(ds, "crosstab")
            oledbAdapter.Dispose()
            user_connection.Close()
            
            crosstab_dt = ds.Tables("crosstab")    
            
            Dim strTest As String
            Dim dc As Data.DataColumn
                For Each dc In crosstab_dt.Columns
                    strTest = dc.ColumnName
                Next
            
            crosstab_dt.Columns(4).ColumnName = "Instructor" ' Changes "ALS" Header to "Instructor"
            crosstab_dt.Columns.Remove(crosstab_dt.Columns(4).ColumnName)  ' Hides ALS/Instructor Column
            
            Dim i As Integer
            Dim SplitResults() As String

'             Dim commaFile As String = ReadTextFile("/Managers/ColumnsFile.csv")  
'             SplitResults = commaFile.Split(",")
' 
'             For i = 0 To UBound(SplitResults)
'                 If crosstab_dt.Columns.Contains(SplitResults(i)) Then
'                     crosstab_dt.columns.Remove(SplitResults(i))
'                 End If
'             Next i
            
            Dim dv As New Data.DataView(crosstab_dt)
            dv.RowFilter = "Affiliation = 'DFRS'"
             
            Return dv 
            
    End Function
    
    Public Function ReadTextFile(ByVal CSV As String) As String

        Dim fi As New FileInfo(Server.MapPath(CSV))
        Dim sr As StreamReader = fi.OpenText()
        Dim body As String = sr.ReadToEnd
        sr.Close()
        Return body
    End Function

	Public Shared Function GetUserAutoNum(ByVal username As String) As String
		Dim objConn As Data.OleDb.OleDbConnection
            Dim objCmd As Data.OleDb.OleDbCommand
            Dim objRdr As Data.OleDb.OleDbDataReader
            Dim userAN As String
            Dim strConnection As String = ConfigurationManager.ConnectionStrings("AccessSubsite").ToString
            objConn = New Data.OleDb.OleDbConnection(strConnection)
            objCmd = New Data.OleDb.OleDbCommand("SELECT * FROM UsersDataTbl WHERE [UserName] = """ & username & """", objConn)
        
            objConn.Open()
            objRdr = objCmd.ExecuteReader()
        
            While objRdr.Read()
                userAN = objRdr.Item("UID")
            End While
                    
            objRdr.Close()
            objConn.Close()

            Return userAN
                
            objConn.Close() 
    End Function


    Public Shared Function UpdateActivityDate(ByVal username As String) As Boolean
        Dim connStr As String = ConfigurationManager.AppSettings.Get("SubsiteConn")
        Dim conn As New Data.OleDb.OleDbConnection(connStr)

        conn.Open()
        Dim sql As String = "UPDATE [Users] SET [LastActivityDate] = #" & Date.Now.AddHours(3).ToString() & "# " & _
             " WHERE [UserName] = """ & username & """"
        Dim comm As New Data.OleDb.OleDbCommand(sql, conn)
        Dim result As Integer = comm.ExecuteNonQuery()
        conn.Close()
        Return True
    End Function
    
    
     Public Shared Function RecordExist(ByVal username As String, ByVal classname As String) As Boolean
        Dim connStr As String = ConfigurationManager.AppSettings.Get("SubsiteConn")
        Dim conn As New Data.OleDb.OleDbConnection(connStr)
        Dim CN As String = classname
        Dim UN As String = username 
        Dim sql As String = "SELECT COUNT(*) FROM EnrollmentsTbl " & _
             " WHERE [UserName] = """ & UN & """ AND [ClassName] = """ & CN & """"    
             	' AND Enrolled = False" (***add to track error after all entries from on-line are converted to enrolled =False***)
        Dim DBCommand As New Data.OleDb.OleDbCommand(sql, conn)

            conn.Open()
            Dim RecordCount As Integer = CInt(DBCommand.ExecuteScalar())
            conn.Close()
				If RecordCount > 0 Then
            		Return True
            	Else 
            		Return False
            	End If
    End Function
    
    
    Public Shared Sub SendMail(ByVal EmailTo As String, ByVal MailBody As String, ByVal MailSubject As String)       
        Dim BlindEmail As String = ConfigurationManager.AppSettings.Get("BlindEmail")
        Dim EmailFrom As String = ConfigurationManager.AppSettings.Get("EmailFrom")
        
        'On/Off switch to send mail receipt or not.
        Dim sendMailOnSwitch As Boolean = True
        If sendMailOnSwitch Then
            Dim ToAddress As String = EmailTo
       		Dim mm As New MailMessage(EmailFrom, ToAddress)
            'mm.Bcc.Add(New MailAddress(BlindEmail))
            mm.Subject = MailSubject
            mm.Body = MailBody
            mm.IsBodyHtml = True

            Dim smtp As New SmtpClient
            Dim basicAuthenticationInfo As New System.Net.NetworkCredential("webmaster@mcfrsit.com", "yalcrab")
            smtp.Host = "localhost"
            smtp.UseDefaultCredentials = False
            smtp.Credentials = basicAuthenticationInfo
            smtp.Send(mm)
        End If
    End Sub
    
    
    Public Shared Function GetEmailAddress(ByVal username As String)
            Dim objConn As Data.OleDb.OleDbConnection
            Dim objCmd As Data.OleDb.OleDbCommand
            Dim objRdr As Data.OleDb.OleDbDataReader
            Dim strConnection As String = ConfigurationManager.AppSettings.Get("SubsiteConn")
            Dim sql As String = "Select * From Users WHERE username=@username"
            objConn = New Data.OleDb.OleDbConnection(strConnection)
            objCmd = New Data.OleDb.OleDbCommand(sql, objConn)
            objCmd.Parameters.AddWithValue("@username", username)
            objConn.Open()
            	objRdr = objCmd.ExecuteReader()
            	While objRdr.Read()
                	Dim EM As String = objRdr.Item("Email")
                	Return EM
            	End While
            objConn.Close()
            Return Nothing
    End Function

End Class

End Namespace
