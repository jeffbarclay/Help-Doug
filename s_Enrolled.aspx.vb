Imports System.Net.Mail
Partial Class s_Enrolled
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Session("UserID") = User.Identity.Name()
    End Sub
    
    
    Protected Sub GridView1_RowDeleting(ByVal sender As [Object], ByVal e As GridViewDeleteEventArgs)
        
        'e.Cancel = False
        
        Dim ClassName As String = GridView1.Rows(e.RowIndex).Cells(1).Text
        Dim ClassDate As String = GridView1.Rows(e.RowIndex).Cells(2).Text
        Dim ClassTime As String = GridView1.Rows(e.RowIndex).Cells(3).Text
        Dim ClassWaitList As Boolean = GridView1.Rows(e.RowIndex).Cells(4).Text
        Dim UserName As String = User.Identity.Name()
        Dim EmailBody As String = MessageBody(ClassName, ClassDate, ClassTime)
        Dim MaxStudents As Integer = GetMaxStudents(ClassName, ClassDate, ClassTime)
        Dim StudentCount As Integer = GetStudentCount(ClassName, ClassDate, ClassTime)
        
        If (StudentCount > MaxStudents) Then
        	If (ClassWaitList) Then
        		MyLabel2.Text = "(Label2) Deleted Waitlisted Student - no roster impact" 
        		MyLabel4.Text = "(Label4) Num of Students NOT Waitlisted: " & (StudentCount - 1)
        	Else
        		Dim SubmitTime As Date = GetNextWaitListed(ClassName, ClassDate, ClassTime)
        		Dim NumUpdated As Integer = UpdateNextWaitlisted(SubmitTime)
        		Dim WLUN As String = GetWaitListedUserName(SubmitTime)
        		DIm WLUserEmail As String = GetEmailAddress(WLUN)
        		
        		SendMail(WLUserEmail, EmailBody)
        		
        			MyLabel2.Text = "(Label2) Student from waitlist enrolled! " & SubmitTime
        			MyLabel3.Text = "(Label3) # waitlisted record updated?: " & NumUpdated  
        	End If
        End If 
    End Sub
    
    
    Public Function UpdateNextWaitlisted(ByVal EnteredTime As Date)
        Dim connStr As String = ConfigurationManager.AppSettings.Get("TechTrainingConn")
        Dim conn As New Data.OleDb.OleDbConnection(connStr)
        Try
            conn.Open()
            Dim sql As String = "UPDATE [EnrollmentsTbl] SET [Waitlisted] = False " & _
            "WHERE [SubmitTime] = #" & EnteredTime & "#"
            Dim comm As New Data.OleDb.OleDbCommand(sql, conn)
            Dim result As Integer = comm.ExecuteNonQuery
            Return result
        Catch ex As Exception
            If Not conn Is Nothing Then
                conn.Close()
            End If
            Return False
        End Try
    End Function
    
    
    Public Function GetMaxStudents(ByVal ClassName As String, ByVal ClassDate As Date, ByVal ClassTime As String)
        Dim connStr As String = ConfigurationManager.AppSettings.Get("TechTrainingConn")
        Dim conn As New Data.OleDb.OleDbConnection(connStr)
        Try
            conn.Open()
            Dim sql As String = "Select MaxStudents FROM [ClassesTbl] " & _
            					" WHERE [ClassName] = """ & ClassName & """" & _
                           		" AND [ClassDate] = #" & ClassDate & "#" & _
                           		" AND [ClassTime] = """ & ClassTime & """"
            Dim comm As New Data.OleDb.OleDbCommand(sql, conn)
            Dim result As Integer = comm.ExecuteScalar()
            MyLabel.Text = "(Label) MaxStudents: " & result
            Return result
        Catch ex As Exception
            If Not conn Is Nothing Then
                conn.Close()
            End If
        End Try
    
    End Function
    
    
    Public Function GetStudentCount(ByVal ClassName As String, ByVal ClassDate As Date, ByVal ClassTime As String)
        Dim connStr As String = ConfigurationManager.AppSettings.Get("TechTrainingConn")
        Dim conn As New Data.OleDb.OleDbConnection(connStr)
        Dim sql As String = "SELECT COUNT(*) FROM [EnrollmentsTbl]" & _
                           " WHERE [ClassName] = """ & ClassName & """" & _
                           " AND [ClassDate] = #" & ClassDate & "#" & _
                           " AND [ClassTime] = """ & ClassTime & """" & _
                           " AND [Enrolled] = True" & _
                           " AND [WaitListed] = False"
        Dim DBCommand As New Data.OleDb.OleDbCommand(sql, conn)
        Try
            conn.Open()
            Dim StudentCount As Integer = CInt(DBCommand.ExecuteScalar())
            conn.Close()
			MyLabel4.Text = "(Label4) Num of Students NOT Waitlisted: " & (StudentCount - 1)
            Return StudentCount

        Catch ex As Exception
            Response.Write(ex)
        Finally
            conn.Close()
        End Try

    End Function
      
    
    Public Function GetNextWaitListed(ByVal ClassName As String, ByVal ClassDate As Date, ByVal ClassTime As String) As String
        Dim connStr As String = ConfigurationManager.AppSettings.Get("TechTrainingConn")
        Dim conn As New Data.OleDb.OleDbConnection(connStr)
        
        Try
        	conn.Open()
        	Dim sql As String = "SELECT Min(SubmitTime) FROM [EnrollmentsTbl]" & _
                           " WHERE [ClassName] = """ & ClassName & """" & _
                           " AND [ClassDate] = #" & ClassDate & "#" & _
                           " AND [ClassTime] = """ & ClassTime & """" & _
                           " AND [Waitlisted] = True" & _
                           " AND [Completed] = False" & _
                           " AND [Enrolled] = True" 
            Dim comm As New Data.OleDb.OleDbCommand(sql, conn)
            Dim Obj As Object = comm.ExecuteScalar()
                If (Obj IsNot Nothing) AndAlso (Obj IsNot DBNull.Value) Then
                    Dim matches As String = Obj.ToString
            		Dim result As Date = Convert.ToDateTime(matches)
            		'MyLabel.Text = "(Label) The String Object is: " & matches & " the result Date is: " & result
        			Return result
        		Else
        			Dim result As Date = Convert.ToDateTime("01/01/1900")
        			Return result
            	End If
            
        Catch ex As Exception
            Response.Write(ex)
            If Not conn Is Nothing Then
                conn.Close()
            End If
        Finally
            conn.Close()
        End Try
        
    End Function
    

    Public Function GetWaitListedUserName(ByVal SubmitTime As Date) AS String
        Dim connStr As String = ConfigurationManager.AppSettings.Get("TechTrainingConn")
        Dim conn As New Data.OleDb.OleDbConnection(connStr)
        Try
            conn.Open()
            Dim sql As String = "Select UserName FROM [EnrollmentsTbl] WHERE [SubmitTime] = #" & SubmitTime & "#" 
            Dim comm As New Data.OleDb.OleDbCommand(sql, conn)
            Dim result As String = comm.ExecuteScalar()
            Return result
        Catch ex As Exception
        	Response.Write(ex)
            If Not conn Is Nothing Then
                conn.Close()
            End If
        Finally
            conn.Close()
        End Try
    
    End Function
    
    
    Public Function GetEmailAddress(ByVal UserName As String) AS String
        Dim connStr As String = ConfigurationManager.AppSettings.Get("TechTrainingConn")
        Dim conn As New Data.OleDb.OleDbConnection(connStr)
        Try
            conn.Open()
            Dim sql As String = "Select Email FROM [Users] WHERE [UserName] = """ & UserName & """" 
            Dim comm As New Data.OleDb.OleDbCommand(sql, conn)
            Dim result As String = comm.ExecuteScalar()
            Return result
        Catch ex As Exception
        	Response.Write(ex)
            If Not conn Is Nothing Then
                conn.Close()
            End If
        Finally
            conn.Close()
        End Try
    
    End Function
    
    
    Protected Sub SendMail(ByVal EmailTo As String, ByVal MailBody As String)
        Dim BlindEmail As String = ConfigurationManager.AppSettings.Get("BlindEmail")
        Dim EmailFrom As String = ConfigurationManager.AppSettings.Get("EmailFrom")
        'On/Off switch to send mail receipt or not.
        Dim sendMailOnSwitch As Boolean = True
        If sendMailOnSwitch Then

            Dim ToAddress As String = EmailTo
            Try
                Dim mm As New MailMessage(EmailFrom, ToAddress)

                mm.Bcc.Add(New MailAddress(BlindEmail))
                mm.Subject = "Moved from Wait List to Enrolled (MCFRSIT.COM)"
                mm.Body = MailBody
                mm.IsBodyHtml = True

                Dim smtp As New SmtpClient
                Dim basicAuthenticationInfo As _
                   New System.Net.NetworkCredential("webmaster@mcfrsit.com", "yalcrab")
                'Put your own, or your ISPs, mail server name on this next line
                smtp.Host = "localhost"
                smtp.UseDefaultCredentials = False
                smtp.Credentials = basicAuthenticationInfo
                smtp.Send(mm)
                MyLabel1.Text = "<br>Notification of this change has been emailed to: " & ToAddress & "<br>"
            Catch ex As Exception
                MyLabel1.Text = "<br>No email was sent to address on file, please check your your system Admin.<br>."
            End Try

        End If
    End Sub


    Public Function MessageBody(ByVal ClassName As String, ByVal ClassDate As String, ByVal ClassTime As String) As String
        Dim htmltext As String
        htmltext = "You have been moved from wait listed statice to enrolled for the following class:<br>"
        htmltext = htmltext & "Class Name: " & ClassName & "<br>"
        htmltext = htmltext & "Class Date: " & ClassDate & "<br>"
        htmltext = htmltext & "Class Time: " & ClassTime & "<br>"
        htmltext = htmltext & "Use this information to login to the MCFRSIT Web Site."
        htmltext = htmltext & "<br>Click <a href='http://www.mcfrsit.com'>here</a> to access."

        Return htmltext

    End Function
    
    
End Class
