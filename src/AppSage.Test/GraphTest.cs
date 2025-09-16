using AppSage.Core.ComplexType.Graph;

namespace AppSage.Test
{
    public class GraphTest
    {
        // Define code metric constants
        private static class CodeMetric
        {
            public const string LinesOfCode = "LinesOfCode";
            public const string NumberOfMethods = "NumberOfMethods";
            public const string NumberOfProperties = "NumberOfProperties";
            public const string Complexity = "CyclomaticComplexity";
            public const string Dependencies = "Dependencies";
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            //var graph = new DirectedGraph();

            //// Create class nodes with metrics
            //var userClass = graph.AddOrUpdateNode("UserClass");
            //userClass.Attributes[CodeMetric.LinesOfCode] = "120";
            //userClass.Attributes[CodeMetric.NumberOfMethods] = "8";
            //userClass.Attributes[CodeMetric.NumberOfProperties] = "5";
            //userClass.Attributes[CodeMetric.Complexity] = "18";
            //userClass.Attributes[CodeMetric.Dependencies] = "3";
            
            //var authService = graph.AddOrUpdateNode( "AuthenticationService");
            //authService.Attributes[CodeMetric.LinesOfCode] = "175";
            //authService.Attributes[CodeMetric.NumberOfMethods] = "12";
            //authService.Attributes[CodeMetric.NumberOfProperties] = "3";
            //authService.Attributes[CodeMetric.Complexity] = "22";
            //authService.Attributes[CodeMetric.Dependencies] = "4";
            
            //var userRepo = graph.AddOrUpdateNode( "UserRepository");
            //userRepo.Attributes[CodeMetric.LinesOfCode] = "150";
            //userRepo.Attributes[CodeMetric.NumberOfMethods] = "9";
            //userRepo.Attributes[CodeMetric.NumberOfProperties] = "2";
            //userRepo.Attributes[CodeMetric.Complexity] = "14";
            //userRepo.Attributes[CodeMetric.Dependencies] = "2";
            
            //var dbContext = graph.AddOrUpdateNode( "DatabaseContext");
            //dbContext.Attributes[CodeMetric.LinesOfCode] = "200";
            //dbContext.Attributes[CodeMetric.NumberOfMethods] = "15";
            //dbContext.Attributes[CodeMetric.NumberOfProperties] = "8";
            //dbContext.Attributes[CodeMetric.Complexity] = "24";
            //dbContext.Attributes[CodeMetric.Dependencies] = "1";
            
            //var logger = graph.AddOrUpdateNode( "Logger");
            //logger.Attributes[CodeMetric.LinesOfCode] = "85";
            //logger.Attributes[CodeMetric.NumberOfMethods] = "6";
            //logger.Attributes[CodeMetric.NumberOfProperties] = "2";
            //logger.Attributes[CodeMetric.Complexity] = "8";
            //logger.Attributes[CodeMetric.Dependencies] = "1";
            
            //var validation = graph.AddOrUpdateNode( "ValidationService");
            //validation.Attributes[CodeMetric.LinesOfCode] = "110";
            //validation.Attributes[CodeMetric.NumberOfMethods] = "7";
            //validation.Attributes[CodeMetric.NumberOfProperties] = "1";
            //validation.Attributes[CodeMetric.Complexity] = "19";
            //validation.Attributes[CodeMetric.Dependencies] = "2";
            
            //var emailService = graph.AddOrUpdateNode( "EmailService");
            //emailService.Attributes[CodeMetric.LinesOfCode] = "130";
            //emailService.Attributes[CodeMetric.NumberOfMethods] = "8";
            //emailService.Attributes[CodeMetric.NumberOfProperties] = "4";
            //emailService.Attributes[CodeMetric.Complexity] = "15";
            //emailService.Attributes[CodeMetric.Dependencies] = "3";
            
            //var tokenService = graph.AddOrUpdateNode( "TokenService");
            //tokenService.Attributes[CodeMetric.LinesOfCode] = "95";
            //tokenService.Attributes[CodeMetric.NumberOfMethods] = "6";
            //tokenService.Attributes[CodeMetric.NumberOfProperties] = "2";
            //tokenService.Attributes[CodeMetric.Complexity] = "13";
            //tokenService.Attributes[CodeMetric.Dependencies] = "2";
            
            //var config = graph.AddOrUpdateNode( "ConfigurationManager");
            //config.Attributes[CodeMetric.LinesOfCode] = "70";
            //config.Attributes[CodeMetric.NumberOfMethods] = "5";
            //config.Attributes[CodeMetric.NumberOfProperties] = "12";
            //config.Attributes[CodeMetric.Complexity] = "6";
            //config.Attributes[CodeMetric.Dependencies] = "0";
            
            //var encryption = graph.AddOrUpdateNode( "EncryptionService");
            //encryption.Attributes[CodeMetric.LinesOfCode] = "115";
            //encryption.Attributes[CodeMetric.NumberOfMethods] = "8";
            //encryption.Attributes[CodeMetric.NumberOfProperties] = "3";
            //encryption.Attributes[CodeMetric.Complexity] = "17";
            //encryption.Attributes[CodeMetric.Dependencies] = "1";

            //// Create method nodes for UserClass with metrics
            //var login = graph.AddOrUpdateNode("Login");
            //login.Attributes[CodeMetric.LinesOfCode] = "25";
            //login.Attributes[CodeMetric.Complexity] = "4";
            //login.Attributes[CodeMetric.Dependencies] = "2";
            
            //var register = graph.AddOrUpdateNode("Register");
            //register.Attributes[CodeMetric.LinesOfCode] = "35";
            //register.Attributes[CodeMetric.Complexity] = "6";
            //register.Attributes[CodeMetric.Dependencies] = "4";
            
            //var validateUser = graph.AddOrUpdateNode("ValidateUser");
            //validateUser.Attributes[CodeMetric.LinesOfCode] = "18";
            //validateUser.Attributes[CodeMetric.Complexity] = "3";
            //validateUser.Attributes[CodeMetric.Dependencies] = "1";
            
            //var resetPassword = graph.AddOrUpdateNode("ResetPassword");
            //resetPassword.Attributes[CodeMetric.LinesOfCode] = "30";
            //resetPassword.Attributes[CodeMetric.Complexity] = "5";
            //resetPassword.Attributes[CodeMetric.Dependencies] = "3";
            
            //var updateProfile = graph.AddOrUpdateNode("UpdateProfile");
            //updateProfile.Attributes[CodeMetric.LinesOfCode] = "22";
            //updateProfile.Attributes[CodeMetric.Complexity] = "3";
            //updateProfile.Attributes[CodeMetric.Dependencies] = "2";

            //// Create method nodes for AuthenticationService with metrics
            //var authenticate = graph.AddOrUpdateNode("Authenticate");
            //authenticate.Attributes[CodeMetric.LinesOfCode] = "28";
            //authenticate.Attributes[CodeMetric.Complexity] = "5";
            //authenticate.Attributes[CodeMetric.Dependencies] = "3";
            
            //var generateToken = graph.AddOrUpdateNode("GenerateToken");
            //generateToken.Attributes[CodeMetric.LinesOfCode] = "15";
            //generateToken.Attributes[CodeMetric.Complexity] = "2";
            //generateToken.Attributes[CodeMetric.Dependencies] = "1";
            
            //var validateToken = graph.AddOrUpdateNode("ValidateToken");
            //validateToken.Attributes[CodeMetric.LinesOfCode] = "20";
            //validateToken.Attributes[CodeMetric.Complexity] = "4";
            //validateToken.Attributes[CodeMetric.Dependencies] = "2";
            
            //var refreshToken = graph.AddOrUpdateNode("RefreshToken");
            //refreshToken.Attributes[CodeMetric.LinesOfCode] = "18";
            //refreshToken.Attributes[CodeMetric.Complexity] = "3";
            //refreshToken.Attributes[CodeMetric.Dependencies] = "1";
            
            //var revokeToken = graph.AddOrUpdateNode("RevokeToken");
            //revokeToken.Attributes[CodeMetric.LinesOfCode] = "12";
            //revokeToken.Attributes[CodeMetric.Complexity] = "2";
            //revokeToken.Attributes[CodeMetric.Dependencies] = "1";

            //// Create method nodes for UserRepository with metrics
            //var getUser = graph.AddOrUpdateNode("GetUser");
            //getUser.Attributes[CodeMetric.LinesOfCode] = "14";
            //getUser.Attributes[CodeMetric.Complexity] = "2";
            //getUser.Attributes[CodeMetric.Dependencies] = "1";
            
            //var createUser = graph.AddOrUpdateNode("CreateUser");
            //createUser.Attributes[CodeMetric.LinesOfCode] = "25";
            //createUser.Attributes[CodeMetric.Complexity] = "3";
            //createUser.Attributes[CodeMetric.Dependencies] = "3";
            
            //var updateUser = graph.AddOrUpdateNode("UpdateUser");
            //updateUser.Attributes[CodeMetric.LinesOfCode] = "22";
            //updateUser.Attributes[CodeMetric.Complexity] = "3";
            //updateUser.Attributes[CodeMetric.Dependencies] = "2";
            
            //var deleteUser = graph.AddOrUpdateNode("DeleteUser");
            //deleteUser.Attributes[CodeMetric.LinesOfCode] = "15";
            //deleteUser.Attributes[CodeMetric.Complexity] = "2";
            //deleteUser.Attributes[CodeMetric.Dependencies] = "2";
            
            //var getUserByEmail = graph.AddOrUpdateNode("GetUserByEmail");
            //getUserByEmail.Attributes[CodeMetric.LinesOfCode] = "16";
            //getUserByEmail.Attributes[CodeMetric.Complexity] = "2";
            //getUserByEmail.Attributes[CodeMetric.Dependencies] = "1";

            //// Create method nodes for ValidationService with metrics
            //var validateEmail = graph.AddOrUpdateNode("ValidateEmail");
            //validateEmail.Attributes[CodeMetric.LinesOfCode] = "18";
            //validateEmail.Attributes[CodeMetric.Complexity] = "4";
            //validateEmail.Attributes[CodeMetric.Dependencies] = "0";
            
            //var validatePassword = graph.AddOrUpdateNode("ValidatePassword");
            //validatePassword.Attributes[CodeMetric.LinesOfCode] = "22";
            //validatePassword.Attributes[CodeMetric.Complexity] = "5";
            //validatePassword.Attributes[CodeMetric.Dependencies] = "0";
            
            //var validateInput = graph.AddOrUpdateNode("ValidateInput");
            //validateInput.Attributes[CodeMetric.LinesOfCode] = "20";
            //validateInput.Attributes[CodeMetric.Complexity] = "3";
            //validateInput.Attributes[CodeMetric.Dependencies] = "1";

            //// Create method nodes for EmailService
            //var sendEmail = graph.AddOrUpdateNode("SendEmail");
            //sendEmail.Attributes[CodeMetric.LinesOfCode] = "24";
            //sendEmail.Attributes[CodeMetric.Complexity] = "3";
            //sendEmail.Attributes[CodeMetric.Dependencies] = "2";
            
            //var buildTemplate = graph.AddOrUpdateNode("BuildTemplate");
            //buildTemplate.Attributes[CodeMetric.LinesOfCode] = "30";
            //buildTemplate.Attributes[CodeMetric.Complexity] = "4";
            //buildTemplate.Attributes[CodeMetric.Dependencies] = "1";
            
            //var queueEmail = graph.AddOrUpdateNode("QueueEmail");
            //queueEmail.Attributes[CodeMetric.LinesOfCode] = "15";
            //queueEmail.Attributes[CodeMetric.Complexity] = "2";
            //queueEmail.Attributes[CodeMetric.Dependencies] = "1";
            
            //var logEmailStatus = graph.AddOrUpdateNode("LogEmailStatus");
            //logEmailStatus.Attributes[CodeMetric.LinesOfCode] = "12";
            //logEmailStatus.Attributes[CodeMetric.Complexity] = "1";
            //logEmailStatus.Attributes[CodeMetric.Dependencies] = "1";

            //// Create method nodes for TokenService
            //var createToken = graph.AddOrUpdateNode("CreateToken");
            //createToken.Attributes[CodeMetric.LinesOfCode] = "20";
            //createToken.Attributes[CodeMetric.Complexity] = "2";
            //createToken.Attributes[CodeMetric.Dependencies] = "1";
            
            //var decodeToken = graph.AddOrUpdateNode("DecodeToken");
            //decodeToken.Attributes[CodeMetric.LinesOfCode] = "18";
            //decodeToken.Attributes[CodeMetric.Complexity] = "3";
            //decodeToken.Attributes[CodeMetric.Dependencies] = "1";
            
            //var validateExpiry = graph.AddOrUpdateNode("ValidateExpiry");
            //validateExpiry.Attributes[CodeMetric.LinesOfCode] = "14";
            //validateExpiry.Attributes[CodeMetric.Complexity] = "2";
            //validateExpiry.Attributes[CodeMetric.Dependencies] = "0";
            
            //var storeToken = graph.AddOrUpdateNode("StoreToken");
            //storeToken.Attributes[CodeMetric.LinesOfCode] = "16";
            //storeToken.Attributes[CodeMetric.Complexity] = "2";
            //storeToken.Attributes[CodeMetric.Dependencies] = "1";

            //// Create method nodes for EncryptionService
            //var encrypt = graph.AddOrUpdateNode("Encrypt");
            //encrypt.Attributes[CodeMetric.LinesOfCode] = "18";
            //encrypt.Attributes[CodeMetric.Complexity] = "2";
            //encrypt.Attributes[CodeMetric.Dependencies] = "0";
            
            //var decrypt = graph.AddOrUpdateNode("Decrypt");
            //decrypt.Attributes[CodeMetric.LinesOfCode] = "19";
            //decrypt.Attributes[CodeMetric.Complexity] = "2";
            //decrypt.Attributes[CodeMetric.Dependencies] = "0";
            
            //var hash = graph.AddOrUpdateNode("Hash");
            //hash.Attributes[CodeMetric.LinesOfCode] = "15";
            //hash.Attributes[CodeMetric.Complexity] = "1";
            //hash.Attributes[CodeMetric.Dependencies] = "0";
            
            //var verifyHash = graph.AddOrUpdateNode("VerifyHash");
            //verifyHash.Attributes[CodeMetric.LinesOfCode] = "16";
            //verifyHash.Attributes[CodeMetric.Complexity] = "2";
            //verifyHash.Attributes[CodeMetric.Dependencies] = "0";

            //// Create method nodes for Logger
            //var logInfo = graph.AddOrUpdateNode("LogInfo");
            //logInfo.Attributes[CodeMetric.LinesOfCode] = "12";
            //logInfo.Attributes[CodeMetric.Complexity] = "1";
            //logInfo.Attributes[CodeMetric.Dependencies] = "0";
            
            //var logError = graph.AddOrUpdateNode("LogError");
            //logError.Attributes[CodeMetric.LinesOfCode] = "15";
            //logError.Attributes[CodeMetric.Complexity] = "1";
            //logError.Attributes[CodeMetric.Dependencies] = "0";
            
            //var logWarning = graph.AddOrUpdateNode("LogWarning");
            //logWarning.Attributes[CodeMetric.LinesOfCode] = "12";
            //logWarning.Attributes[CodeMetric.Complexity] = "1";
            //logWarning.Attributes[CodeMetric.Dependencies] = "0";
            
            //var logDebug = graph.AddOrUpdateNode("LogDebug");
            //logDebug.Attributes[CodeMetric.LinesOfCode] = "12";
            //logDebug.Attributes[CodeMetric.Complexity] = "1";
            //logDebug.Attributes[CodeMetric.Dependencies] = "0";
            
            //var flushLogs = graph.AddOrUpdateNode("FlushLogs");
            //flushLogs.Attributes[CodeMetric.LinesOfCode] = "20";
            //flushLogs.Attributes[CodeMetric.Complexity] = "2";
            //flushLogs.Attributes[CodeMetric.Dependencies] = "0";

            //// Create method nodes for DatabaseContext
            //var connect = graph.AddOrUpdateNode("Connect");
            //connect.Attributes[CodeMetric.LinesOfCode] = "22";
            //connect.Attributes[CodeMetric.Complexity] = "3";
            //connect.Attributes[CodeMetric.Dependencies] = "0";
            
            //var executeQuery = graph.AddOrUpdateNode("ExecuteQuery");
            //executeQuery.Attributes[CodeMetric.LinesOfCode] = "30";
            //executeQuery.Attributes[CodeMetric.Complexity] = "4";
            //executeQuery.Attributes[CodeMetric.Dependencies] = "0";
            
            //var beginTransaction = graph.AddOrUpdateNode("BeginTransaction");
            //beginTransaction.Attributes[CodeMetric.LinesOfCode] = "15";
            //beginTransaction.Attributes[CodeMetric.Complexity] = "2";
            //beginTransaction.Attributes[CodeMetric.Dependencies] = "0";
            
            //var commit = graph.AddOrUpdateNode("Commit");
            //commit.Attributes[CodeMetric.LinesOfCode] = "14";
            //commit.Attributes[CodeMetric.Complexity] = "2";
            //commit.Attributes[CodeMetric.Dependencies] = "0";
            
            //var rollback = graph.AddOrUpdateNode("Rollback");
            //rollback.Attributes[CodeMetric.LinesOfCode] = "16";
            //rollback.Attributes[CodeMetric.Complexity] = "2";
            //rollback.Attributes[CodeMetric.Dependencies] = "0";

            //// Add edges for class dependencies
            //graph.AddOrUpdateEdge(userClass, authService, "Uses");
            //graph.AddOrUpdateEdge(userClass, validation, "Uses");
            //graph.AddOrUpdateEdge(userClass, logger, "Uses");
            //graph.AddOrUpdateEdge(authService, userRepo, "Uses");
            //graph.AddOrUpdateEdge(authService, tokenService, "Uses");
            //graph.AddOrUpdateEdge(authService, logger, "Uses");
            //graph.AddOrUpdateEdge(userRepo, dbContext, "Uses");
            //graph.AddOrUpdateEdge(userRepo, logger, "Uses");
            //graph.AddOrUpdateEdge(tokenService, encryption, "Uses");
            //graph.AddOrUpdateEdge(tokenService, config, "Uses");
            //graph.AddOrUpdateEdge(emailService, logger, "Uses");
            //graph.AddOrUpdateEdge(emailService, config, "Uses");

            //// Add method dependencies for Login flow
            //graph.AddOrUpdateEdge(login, validateUser, "Calls");
            //graph.AddOrUpdateEdge(login, authenticate, "Calls");
            //graph.AddOrUpdateEdge(authenticate, getUser, "Calls");
            //graph.AddOrUpdateEdge(authenticate, verifyHash, "Calls");
            //graph.AddOrUpdateEdge(authenticate, generateToken, "Calls");
            //graph.AddOrUpdateEdge(authenticate, logInfo, "Calls");
            //graph.AddOrUpdateEdge(generateToken, createToken, "Calls");
            //graph.AddOrUpdateEdge(createToken, encrypt, "Calls");
            //graph.AddOrUpdateEdge(getUser, executeQuery, "Calls");

            //// Add method dependencies for Register flow
            //graph.AddOrUpdateEdge(register, validateEmail, "Calls");
            //graph.AddOrUpdateEdge(register, validatePassword, "Calls");
            //graph.AddOrUpdateEdge(register, getUserByEmail, "Calls");
            //graph.AddOrUpdateEdge(register, hash, "Calls");
            //graph.AddOrUpdateEdge(register, createUser, "Calls");
            //graph.AddOrUpdateEdge(register, sendEmail, "Calls");
            //graph.AddOrUpdateEdge(createUser, executeQuery, "Calls");
            //graph.AddOrUpdateEdge(createUser, beginTransaction, "Calls");
            //graph.AddOrUpdateEdge(createUser, commit, "Calls");
            //graph.AddOrUpdateEdge(sendEmail, buildTemplate, "Calls");
            //graph.AddOrUpdateEdge(sendEmail, queueEmail, "Calls");

            //// Add method dependencies for Reset Password flow
            //graph.AddOrUpdateEdge(resetPassword, validateEmail, "Calls");
            //graph.AddOrUpdateEdge(resetPassword, getUserByEmail, "Calls");
            //graph.AddOrUpdateEdge(resetPassword, sendEmail, "Calls");
            //graph.AddOrUpdateEdge(resetPassword, generateToken, "Calls");
            //graph.AddOrUpdateEdge(resetPassword, logInfo, "Calls");

            //// Add exception handling and logging edges
            //graph.AddOrUpdateEdge(login, logError, "Exception");
            //graph.AddOrUpdateEdge(register, logError, "Exception");
            //graph.AddOrUpdateEdge(createUser, rollback, "OnFailure");
            //graph.AddOrUpdateEdge(authenticate, logWarning, "OnFailure");

            ////json serialize the graph
            //var json = System.Text.Json.JsonSerializer.Serialize(graph, new System.Text.Json.JsonSerializerOptions
            //{
            //    WriteIndented = true
            //});
            //File.WriteAllText(@"c:\temp\graph.json", json);
            Assert.Pass();
        }
    }
}