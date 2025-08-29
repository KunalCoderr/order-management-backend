Imports System
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Security.Cryptography
Imports System.Configuration

Public Module CommonUtils

    ' Validates if a string is a valid email address
    Public Function IsValidEmail(email As String) As Boolean
        If String.IsNullOrWhiteSpace(email) Then Return False
        Dim pattern As String = "^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"
        Return Regex.IsMatch(email, pattern)
    End Function

    ' Validates if a string is not null, empty or whitespace
    Public Function IsNotNullOrEmpty(value As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(value)
    End Function

    ' Logs a message to a text file with timestamp
    Public Sub LogMessage(message As String, Optional logFilePath As String = "application.log")
        Try
            Dim logFolderPath As String = "C:\logs"

            ' Ensure the Logs folder exists; create if not
            If Not Directory.Exists(logFolderPath) Then
                Directory.CreateDirectory(logFolderPath)
            End If

            Dim filePath As String = Path.Combine(logFolderPath, logFilePath)
            Dim logEntry As String = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}"

            File.AppendAllText(filePath, logEntry & Environment.NewLine)
        Catch ex As Exception
            ' Fail silently or handle logging error if needed
        End Try
    End Sub

    ' Formats a string to Title Case (e.g. "hello world" => "Hello World")
    Public Function ToTitleCase(input As String) As String
        If String.IsNullOrWhiteSpace(input) Then Return input
        Dim cultureInfo = Globalization.CultureInfo.CurrentCulture
        Dim textInfo = cultureInfo.TextInfo
        Return textInfo.ToTitleCase(input.ToLower())
    End Function

    ' Parses string to integer safely
    Public Function TryParseInt(value As String, ByRef result As Integer) As Boolean
        Return Integer.TryParse(value, result)
    End Function

    ' Checks if a string matches a given regex pattern
    Public Function MatchesPattern(value As String, pattern As String) As Boolean
        If String.IsNullOrEmpty(value) OrElse String.IsNullOrEmpty(pattern) Then Return False
        Return Regex.IsMatch(value, pattern)
    End Function

    ' Parses string to DateTime safely
    Public Function TryParseDate(value As String, ByRef result As DateTime) As Boolean
        Return DateTime.TryParse(value, result)
    End Function

    ' Returns current UTC time formatted as ISO 8601 string
    Public Function GetCurrentUtcIsoDate() As String
        Return DateTime.UtcNow.ToString("o")
    End Function

    ' Reads an app setting value from config file
    Public Function GetAppSetting(key As String) As String
        Try
            Return ConfigurationManager.AppSettings(key)
        Catch
            Return Nothing
        End Try
    End Function

    ' Encrypts a string using AES (returns base64)
    Public Function EncryptString(plainText As String, key As String) As String
        If String.IsNullOrEmpty(plainText) OrElse String.IsNullOrEmpty(key) Then Return Nothing
        Using aes As Aes = Aes.Create()
            Dim keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32))
            aes.Key = keyBytes
            aes.Mode = CipherMode.CBC
            aes.GenerateIV()
            Dim iv = aes.IV

            Using encryptor = aes.CreateEncryptor()
                Dim plainBytes = Encoding.UTF8.GetBytes(plainText)
                Dim encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length)

                Dim result = New Byte(iv.Length + encryptedBytes.Length - 1) {}
                Buffer.BlockCopy(iv, 0, result, 0, iv.Length)
                Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length)

                Return Convert.ToBase64String(result)
            End Using
        End Using
    End Function

    ' Decrypts a string encrypted with above method
    Public Function DecryptString(cipherText As String, key As String) As String
        If String.IsNullOrEmpty(cipherText) OrElse String.IsNullOrEmpty(key) Then Return Nothing
        Dim fullCipher As Byte()
        Try
            fullCipher = Convert.FromBase64String(cipherText)
        Catch
            Return Nothing
        End Try

        Using aes As Aes = Aes.Create()
            Dim keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32))
            aes.Key = keyBytes
            aes.Mode = CipherMode.CBC

            Dim iv = New Byte(15) {}
            Dim cipher = New Byte(fullCipher.Length - iv.Length - 1) {}

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length)
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length)

            aes.IV = iv

            Using decryptor = aes.CreateDecryptor()
                Try
                    Dim decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length)
                    Return Encoding.UTF8.GetString(decryptedBytes)
                Catch
                    Return Nothing
                End Try
            End Using
        End Using
    End Function

    ' Safely converts object to string
    Public Function SafeToString(obj As Object) As String
        If obj Is Nothing Then Return String.Empty
        Return obj.ToString()
    End Function

    ' Safely converts object to integer with default fallback
    Public Function SafeToInt(obj As Object, Optional defaultValue As Integer = 0) As Integer
        If obj Is Nothing Then Return defaultValue
        Dim result As Integer
        If Integer.TryParse(obj.ToString(), result) Then
            Return result
        Else
            Return defaultValue
        End If
    End Function

    ' Returns a random alphanumeric string of given length
    Public Function GenerateRandomString(length As Integer) As String
        Const chars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
        Dim random = New Random()
        Dim result = New StringBuilder()
        For i As Integer = 1 To length
            result.Append(chars(random.Next(chars.Length)))
        Next
        Return result.ToString()
    End Function

End Module