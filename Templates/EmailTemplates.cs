namespace edusync_backend.Templates
{
    public static class EmailTemplates
    {
        public static string GetWelcomeEmailTemplate(string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to EduSync</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background-color: #4a90e2;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .content {{
            background-color: #ffffff;
            padding: 20px;
            border: 1px solid #dddddd;
            border-radius: 0 0 5px 5px;
        }}
        .button {{
            display: inline-block;
            padding: 12px 24px;
            background-color: #4a90e2;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            margin-top: 20px;
            color: #666666;
            font-size: 12px;
        }}
        .logo {{
            max-width: 150px;
            margin-bottom: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to EduSync!</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>Welcome to EduSync - your gateway to transformative learning experiences! We're thrilled to have you join our community of learners and educators.</p>
            
            <p>With your new account, you can:</p>
            <ul>
                <li>Access a wide range of courses</li>
                <li>Connect with expert instructors</li>
                <li>Track your learning progress</li>
                <li>Participate in interactive assessments</li>
            </ul>

            <p>To get started, simply log in to your account and explore our course catalog.</p>
            
            <a href='http://localhost:3000/login' class='button'>Go to Login</a>

            <p>If you have any questions or need assistance, our support team is here to help.</p>
            
            <p>Best regards,<br>The EduSync Team</p>
        </div>
        <div class='footer'>
            <p>This email was sent to you because you registered for an EduSync account.</p>
            <p>© {DateTime.Now.Year} EduSync. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        public static string GetPasswordResetTemplate(string resetLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background-color: #4a90e2;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .content {{
            background-color: #ffffff;
            padding: 20px;
            border: 1px solid #dddddd;
            border-radius: 0 0 5px 5px;
        }}
        .button {{
            display: inline-block;
            padding: 12px 24px;
            background-color: #4a90e2;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            margin-top: 20px;
            color: #666666;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <h2>Hello,</h2>
            <p>We received a request to reset your password for your EduSync account.</p>
            
            <p>Click the button below to reset your password:</p>
            
            <a href='{resetLink}' class='button'>Reset Password</a>

            <p>If you didn't request this password reset, you can safely ignore this email.</p>
            
            <p>This password reset link will expire in 1 hour.</p>
            
            <p>Best regards,<br>The EduSync Team</p>
        </div>
        <div class='footer'>
            <p>This email was sent to you because you requested a password reset for your EduSync account.</p>
            <p>© {DateTime.Now.Year} EduSync. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
} 