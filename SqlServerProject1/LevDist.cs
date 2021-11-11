using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

using System.Collections;

public partial class TabVal
{
    private class LevResult
    {
        public SqlInt32 Word_ID;
        public SqlInt32 Syn_ID;
        public SqlInt32 Ldist;

        public LevResult(SqlInt32 word_ID, SqlInt32 syn_ID, SqlInt32 ldist)
        {
            Word_ID = word_ID;
            Syn_ID = syn_ID;
            Ldist = ldist;
        }
    }

    public static int CalcLevDist(string str1, string str2)
    {
        if (str1 == null) throw new ArgumentNullException("str1");
        if (str2 == null) throw new ArgumentNullException("str2");
        const int ADD = 1;
        const int DEL = 1;

        int CHANGE;

        int[,] m = new int[str1.Length + 1, str2.Length + 1];

        for (int i = 0; i <= str1.Length; i++) m[i, 0] = i;
        for (int j = 0; j <= str2.Length; j++) m[0, j] = j;

        for (int i = 1; i <= str1.Length; i++)
            for (int j = 1; j <= str2.Length; j++)
            {
                CHANGE = (str1[i - 1] == str2[j - 1]) ? 0 : 1;

                m[i, j] = Math.Min(Math.Min(m[i - 1, j] + DEL,
                m[i, j - 1] + ADD),
                m[i - 1, j - 1] + CHANGE);
            }

        return m[str1.Length, str2.Length];
    }

    [SqlFunction(
        DataAccess = DataAccessKind.Read,
        FillRowMethodName = "FindLevDist_FillRow",
        TableDefinition = " Word_ID int, Syn_ID int, Ldist int")]
    
    public static IEnumerable FindLevDist(string tbName, int tbCount)
    {
        
        ArrayList resultCollection = new ArrayList();
        SqlInt32[] arWID = new SqlInt32[tbCount];
        SqlString[] arALL = new SqlString[tbCount];
        int k = 0;

        using (SqlConnection connection = new SqlConnection("context connection=true"))
        {
            connection.Open();

            using (SqlCommand comm = new SqlCommand("SELECT Id, Name FROM " + tbName , connection))
            {
                using (SqlDataReader Reader = comm.ExecuteReader())
                {
                    while (Reader.Read())
                    {
                        arWID[k] = Reader.GetSqlInt32(0);
                        arALL[k] = Reader.GetSqlString(1);
                        if (k != 0)
                        {
                            for (int i = 0; i < k; i++)
                            {
                                SqlInt32 t = CalcLevDist(Convert.ToString(arALL[i]).Trim(' '), Convert.ToString(arALL[k]).Trim(' '));
                                resultCollection.Add(new LevResult(arWID[i], arWID[k], t));
                            }
                        }
                        k++;
                    }
                }
            }
        }
        return resultCollection;
    }

    public static void FindLevDist_FillRow(object Obj, out SqlInt32 word_ID, out SqlInt32 syn_ID, out SqlInt32 ldist)
    {
        LevResult LevDi = (LevResult)Obj;
        word_ID = LevDi.Word_ID;
        syn_ID = LevDi.Syn_ID;
        ldist = LevDi.Ldist;
    }
};

