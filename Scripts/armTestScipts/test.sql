DECLARE @ChangeScriptName				VARCHAR(500)
DECLARE @ChangeScriptDescription		VARCHAR(2000)
DECLARE @Author							VARCHAR(80)

SET @ChangeScriptName = 'test.sql'
SET @ChangeScriptDescription = 'test inside'
SET @Author = 'Dmytro Vasylenko'

IF NOT EXISTS (SELECT ChangeScript From ChangeScript Where ScriptName = @ChangeScriptName)
BEGIN

 
	/**** ADD DETAILS TO THE TABLE************************************************/
	INSERT INTO ChangeScript(ScriptName, ScriptDescription, CreatedBy)
		VALUES (@ChangeScriptName, @ChangeScriptDescription, @Author)

END
ELSE
BEGIN
	SELECT 'Script has already been run'
	RETURN
END
GO




