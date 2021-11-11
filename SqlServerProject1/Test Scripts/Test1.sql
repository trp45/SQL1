DECLARE @l_count int;
SELECT @l_count = count(*) FROM klstr_temp1
SELECT * FROM FindLevDist('klstr_temp1',@l_count)
go


