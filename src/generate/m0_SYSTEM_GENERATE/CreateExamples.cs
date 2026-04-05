using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;

// this one is a bit of trash

namespace m0_SYSTEM_GENERATE
{
    public class CreateExamples
    {
        private static string randomChars()
        {
            Random r = new Random();
            int x = r.Next(5);

            string xxx = "";

            for (int xx = 0; xx < x; xx++)
                xxx += xx;

            return xxx;
        }

        static void PrintRes(string header, IVertex r)
        {
            m0.MinusZero.Instance.Log(-2, "", "------ " + header);

            if (r != null)
                foreach (IEdge e in r)
                    m0.MinusZero.Instance.Log(-2, "", "    " + e.Meta + " : " + e.To);
        }

        static void q(IVertex baseVertex, string query)
        {
            IVertex re = ((EasyVertex)baseVertex).GetAll(false, query);

            PrintRes(query, re);
        }

        static private void queryTest(IVertex tr)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex bas = tr.AddVertex(null, "bas");

            IVertex ma = tr.AddVertex(null, "ma");
            IVertex mb = tr.AddVertex(null, "mb");

            IVertex maa = tr.AddVertex(null, "maa");
            IVertex mbb = tr.AddVertex(null, "mbb");

            IVertex ma_x = ma.AddVertex(null, "x");

            IVertex maa_y = maa.AddVertex(mb, "y");

            IVertex maa_y_yy = maa_y.AddVertex(mb, "yy");

            IVertex mbb_y = mbb.AddVertex(mb, "y");

            IVertex a = bas.AddVertex(ma, "a");
            IVertex b = bas.AddVertex(mb, "b");

            IVertex c = bas.AddVertex(mbb, "c");

            IVertex aa = a.AddVertex(maa, "aa");
            IVertex aa2 = a.AddVertex(maa, "aa2");

            IVertex bb = b.AddVertex(mbb, "bb");
            IVertex baa = b.AddVertex(mbb, "aa");

            IVertex aaa = aa.AddVertex(ma, "aaa");
            IVertex aaa2 = aa.AddVertex(ma, "aaa2");
            IVertex bbb = bb.AddVertex(mb, "bbb");


            //b.AddEdge(r.Get(false, @"System\Meta*$Inherits"), a);

            m0.MinusZero.Instance.LogLevel = -2;

            q(bas, @"?{mbb:bb}");

            q(bas, @"?{maa{mb:y}:aa}");
            q(bas, @"?{maa{mb:z}:aa}");
            q(bas, @"?{maa{mb:y}:aa{:aaa}}");
            q(bas, @"?{mbb:bb}");
            q(bas, @"?{mbb:}");
            q(bas, @"?{mbb:c}");
            q(bas, @"?{maa:}");
            q(bas, @"?{:aa}");
            q(bas, @"?ma:");
            q(bas, @"?:b");
            q(bas, @"?{{:y}:}");
            q(bas, @"?{{:aaa}}");
            q(bas, @"ma:");
            q(bas, @":b");
            q(bas, @"?");
            q(bas, @"?{maa:aa}");
            q(bas, @"{:x}:");
            q(bas, @"\{:y}:");
            q(bas, @"\{:{:yy}}:");
            q(bas, @"\{:{mb:yy}}:");
            q(bas, @"\{:{mb:yy,mb:z}}:");
            q(bas, @"\");
            q(bas, @":{:aa2}");
            q(bas, @"\:aa");
            q(bas, @"\:aa{:aaa}");
            q(bas, @"\:aa{:aaa,:aaa}");
            q(bas, @"\:aa{:aaa,:aaa,:aaa2,:x}");



            /*   q(bas, @"");
               q(bas, @"mb:b");
               q(bas, @"mbx:b");
               q(bas, @"mb:bx");
               q(bas, @"mbx:");
               q(bas, @"mb:");
               q(bas, @":bx");
               q(bas, @":b");
               q(bas, @":");
               q(bas, @"mb");
               q(bas, @"b");*/


        }

        static void exeTest(IVertex x)
        {
            IVertex code = x.AddVertex(null, "code");

            _exeTest(code);
          /*  _exeTest(code);
            _exeTest(code);
            _exeTest(code);
            _exeTest(code);
            _exeTest(code);
            _exeTest(code);
            _exeTest(code);
            _exeTest(code);
            _exeTest(code);*/
        }

            static void _exeTest(IVertex code)
            {
            IEdge baseEdge_new;
            IEdge code0 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code0,
                "\"Code0\"" + "" +
                "\r\n\tvariable \"A\" @String" , m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new
                );

            IEdge code1 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code1,
                "\"Code1\"" + "" +
                "\r\n\tvariable \"A\" @String" +
                "\r\n\tvariable \"B\" @String" +
                "\r\n\tvariable \"C\" @String" +
                "\r\n\tvariable \"D\" @String" +
                "\r\n\tA +< \"A1\"" +
                "\r\n\tA +< \"A2\"" +
                "\r\n\tA +< \"A3\"" +
                "\r\n\tB +< \"B1\"" +
                "\r\n\tB +< \"B2\"" +
                "\r\n\tB +< \"B3\"" +
                "\r\n\tA <- \"val A\"" +
                "\r\n\tB <- \"val B\"" +
                "\r\n\tC +< A\\ <+> B\\" +
                "\r\n\tD +< C\\ <-> B\\", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new

                );

            IEdge code2 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code2,
                            "\"Code2\"" +
                            "\r\n\tvariable \"Test\" @String" +
                            "\r\n\tvariable \"Test2\" @String" +
                            "\r\n\tvariable \"Test3\" @String" +
                            "\r\n\tvariable \"Magunia\" @String \"5\":\"10\"" +
                            "\r\n\tvariable \"Radek\" @String \"2\":\"2\"" +
                            "\r\n\tRadek += \"Koha\"" +
                            "\r\n\tMagunia+<Radek" +
                            "\r\n\tTest +< Magunia <+> Radek <+> Radek" +
                            "\r\n\tTest2 = Test\\ <+> \"TST\"" +
                            "\r\n\tTest3 = \"TEST#3\"" +
                            "\r\n\tTest3 += Test\\ <-> Radek", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new
                            );

            IEdge code3 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code3,
            "\"Code3\"" +
            "\r\n\tvariable \"A\" @String" +
            "\r\n\tvariable \"B\" @String" +
            "\r\n\tvariable \"C\" @String" +
            "\r\n\tfunction \"X\"(@String \"imie\")" +
            "\r\n\t\tvariable \"B\" @String" +
            "\r\n\t\tB=imie" +
            "\r\n\t\tA=B" +
            "\r\n\t\tB=\"main b\"" +
            "\r\n\t@@X[\"Radek\"]", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new
            );

            IEdge code4 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code4,
            "\"Code4\"" +
            "\r\n\tvariable \"A\" @String" +
            "\r\n\tvariable \"B\" @String" +
            "\r\n\tvariable \"C\" @String" +
            "\r\n\tvariable \"D\" @String" +
            "\r\n\tA = \"A\"" +
            "\r\n\tA += \"A\"" +
            "\r\n\tA += \"B\"" +
            "\r\n\tA += \"True\"" +
            "\r\n\tA += \"True\"" +
            "\r\n\tA += \"1\"" +
            "\r\n\tB = \"A\"" +
            "\r\n\tB += \"B\"" +
            "\r\n\tB += \"A\"" +
            "\r\n\tB += \"1\"" +
            "\r\n\tB += \"0\"" +
            "\r\n\tB += \"1\"" +
            "\r\n\tC = A<B" +
            "\r\n\tD = !C", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new
            );

            //() ! <>

            IEdge code5 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code5,
           "\"Code5\"" +
           "\r\n\tvariable \"A\" @String" +
           "\r\n\tvariable \"B\" @String" +
           "\r\n\tvariable \"C\" @String" +
           "\r\n\tvariable \"D\" @String" +
           "\r\n\tA = \"1\"" +
           "\r\n\tA += \"2\"" +
           "\r\n\tA += \"3\"" +
           "\r\n\tB = (\"1\" + \"2\") * \"3\"" +
           "\r\n\tC = A<\"2\">" +
           "\r\n\tD = !C", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new
           );

            IEdge code6 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code6,
          "\"Code6\"" +
          "\r\n\tvariable \"A\" @String" +
          "\r\n\tvariable \"B\" @String" +
          "\r\n\tvariable \"C\" @String" +
          "\r\n\tvariable \"D\" @String" +
          "\r\n\tfunction \"Add\" @Integer(@Integer \"x\")" +
          "\r\n\t\treturn x+\"1\"" +
          "\r\n\tA = @@Add[\"3\"]", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new
          );

            IEdge code7 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code7,
             "\"Code7\"" +
             "\r\n\tvariable \"A\" @String" +
             "\r\n\tvariable \"B\" @String" +
             "\r\n\tvariable \"C\" @String" +
             "\r\n\tvariable \"D\" @String" +
             "\r\n\tvariable \"E\" @String" +
             "\r\n\tA = \"1\"" +
             "\r\n\tA += \"2\"" +
             "\r\n\tA += \"3\"" +
             "\r\n\tA += \"4\"" +
             "\r\n\tB = \"1\"" +
             "\r\n\tB += \"2\"" +
             "\r\n\tB += \"3\"" +
             "\r\n\tB += \"4\"" +
             "\r\n\tfor vertex \"X\" in A" +
             "\r\n\t\tfor vertex \"Y\" in B" +
             "\r\n\t\t\tC += (X*\"4\") + Y" +
             "\r\n\tfor vertex \"X\" in A" +
             "\r\n\t\tfor vertex \"Y\" in B" +
             "\r\n\t\t\tfor vertex \"Z\" in C" +
             "\r\n\t\t\t\tD += (Z * \"20\") + (X*\"4\") + Y" +
             "\r\n\tfor vertex \"X\" in A" +
             "\r\n\t\tfor vertex \"Y\" in B" +
             "\r\n\t\t\tfor vertex \"W\" in D" +
             "\r\n\t\t\t\tE += (W * \"200\") + (Z * \"20\") + (X*\"4\") + Y", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new
             );

            IEdge code7b = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code7b,
             "\"Code7b\"" +
             "\r\n\tvariable \"A\" @String" +
             "\r\n\tvariable \"B\" @String" +
             "\r\n\tA = \"a\"" +
             "\r\n\tA += \"1\"" +
             "\r\n\tA += \"2\"" +
             "\r\n\tB = \"b\"" +
             "\r\n\tfor vertex \"X\" in A" +
             "\r\n\t\tB += X", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new
             );


            IEdge code8 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code8,
       "\"Code8\"" +
       "\r\n\tvariable \"A\" @String" +
       "\r\n\tvariable \"B\" @String" +
       "\r\n\tA = \"0\"" +
       "\r\n\twhile A <= \"100\"" +
       "\r\n\t\tB += A" +
       "\r\n\t\tA = A + \"1\"", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new

       );

            IEdge code9 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code9,
        "\"Code9\"" +
        "\r\n\tvariable \"a\" @Vertex" +
        "\r\n\ta=@@System", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new);

            IEdge code10 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code10,
        "\"Code10\"" +
        "\r\n\t\"X\"" +
        "\r\n\t\t\"2\" * (\"2\" + \"1\")" +
        "\r\n\tvariable \"A\" @Integer" +
        "\r\n\tvariable \"B\" @String" +
        "\r\n\tvariable \"C\" @Vertex" +
        "\r\n\tvariable \"D\" @Integer" +
        "\r\n\tA = execute($ \\ : X \\ )" +
        "\r\n\tB = generate @@System\\FormalTextLanguage\\ZeroCode ($ \\ : X \\ )" +
        "\r\n\tC = parse @@System\\FormalTextLanguage\\ZeroCode (\"a+b\")" +
        "\r\n\tD = execute(parse(\"A + A\"))", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new
        );

            IEdge code11 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code11,
            "\"Code11\"" +
            "\r\n\t\"x\"" +
            "\r\n\t\t\"a\"" +
            "\r\n\t\t\"z1\"" +
            "\r\n\t\"x\"" +
            "\r\n\t\t\"b\"" +
            "\r\n\t\t\t\"z2\"" +
            "\r\n\t\t\"b\"" +
            "\r\n\t\t\t\"z3\"" +
            "\r\n\tvariable \"a\" @String" +
            "\r\n\ta = ($\\:x<<\"1\">>\\<+>$\\:x<<\"2\">>\\)\\" +
            "\r\n\t@@x<<\"2\">>\\b<<\"2\">>\\z3", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new);

            IEdge code12 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code12,
        "\"Code12\"" +
        "\r\n\tclass \"Person\"" +
        "\r\n\t\tattribute \"Name\" @String \"1\":\"1\"" +
        "\r\n\t\tmethod \"setName\" (@String \"name\")" +
        "\r\n\t\t\tName = name" +
        "\r\n\t\tmethod \"getName\" @String()" +
        "\r\n\t\t\treturn Name" +
    "\r\n\tvariable \"person\" @Person" +
    "\r\n\tvariable \"name\" @String" +
    "\r\n\tperson = new @@Person[]" +
    "\r\n\tperson.setName[\"Rad9k\"]" +
    "\r\n\tname = person.getName[]", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new);

            IEdge code13 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code13,
            "\"Code13\"" +
    "\r\n\tvariable \"mis\" @Boolean" +
    "\r\n\tif (\"1\" + \"2\") == \"3\"" +
    "\r\n\t\tmis = \"True\"" +
    "\r\n\tfunction \"t\" @String(@String \"m\")" +
    "\r\n\t\ttest \"4\"" +
    "\r\n\t\t\tcase \"1\"" +
    "\r\n\t\t\t\treturn \"raz\"" +
    "\r\n\t\t\tcase \"2\"" +
    "\r\n\t\t\t\treturn \"dwa\"" +
    "\r\n\t\t\tcase \"3\"" +
    "\r\n\t\t\t\treturn \"trzy\"" +
    "\r\n\t\t\tfallback" +
    "\r\n\t\t\t\treturn \"def\"" +
    "\r\n\t\t\tcase \"4\"" +
    "\r\n\t\t\t\tif m == \"True\"" +
    "\r\n\t\t\t\t\treturn \"cztery\"" +
    "\r\n\t\treturn \"kupa\"" +
    "\r\n\tvariable \"a\" @String" +
    "\r\n\ta = @@t[mis]", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new);

            //MinusZero.Instance.NewDefaultParser.Parse(code, "\"Code\"\r\n\tvariable \"Magunia\" @String 5:10\r\n\tvariable \"Radek\" @String 2:2\r\n\tRadek += \"Koha\"\r\n\tMagunia=Radek\r\n\tMagunia ~= Radek:Koha");


            IEdge code14 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code14,
            "\"Code14\"" +
    "\r\n\tvariable \"a\" @Boolean" +
    "\r\n\tvariable \"b\" @Boolean" +
    "\r\n\tif (\"1\" + \"2\") == \"3\"" +
    "\r\n\t\ta = \"True\"" +
    "\r\n\tif \"1\" + \"2\" == \"3\"" +
    "\r\n\t\tblock" +
    "\r\n\t\t\ta = \"False\"" +
    "\r\n\t\t\tb = \"True\"", m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new
    );

            IEdge code15 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code15,
            "\"aacode test\"" +
    "\r\n\tnamedblock \"Ablok\"()" +
    "\r\n\t\tvariable \"a\" @Boolean" +
    "\r\n\t\ta = \"True\"" +
    "\r\n\t\tb = \"True\"" +
    "\r\n\tnamedblock \"Bblok\"()" +
    "\r\n\t\tb = \"True\"" +
    "\r\n\tnamedblock \"Cblok\"()" +
    "\r\n\t\treturn Name" +
    "\r\n\tnamedblock \"Dblok\"()" +
    "\r\n\t\tfor vertex \"X\" in x" +
    "\r\n\t\t\tfor vertex \"Y\" in y" +
    "\r\n\t\t\t\tX += Y" +
    "\r\n\tblock" +
    "\r\n\t\ta = \"False\"" +
    "\r\n\t\tb = \"True\"" +
    "\r\n\tblock" +
    "\r\n\t\ta = \"False\"" +
    "\r\n\tblock" +
    "\r\n\t\treturn Name" +
    "\r\n\tif a==b" +
    "\r\n\t\ta = \"True\"" +
    "\r\n\t\tb = \"True\"" +
    "\r\n\tif a==b" +
    "\r\n\t\tb = \"True\"" +
    "\r\n\tif a==b" +
    "\r\n\t\treturn Name" +
    "\r\n\twhile a==b" +
    "\r\n\t\ta = \"True\"" +
    "\r\n\t\tb = \"True\"" +
    "\r\n\twhile a==b" +
    "\r\n\t\tb = \"True\"" +
    "\r\n\twhile a==b" +    
    "\r\n\t\treturn Name" +
    "\r\n\tfunction \"Afunc\"()" +
    "\r\n\t\tvariable \"a\" @Boolean" +
    "\r\n\t\ta = \"True\"" +
    "\r\n\t\tb = \"True\"" +
    "\r\n\tfunction \"Bfunc\"()" +
    "\r\n\t\tb = \"True\"" +
    "\r\n\tfunction \"Cfunc\"()" +
    "\r\n\t\treturn Name" +    
    "\r\n\tfunction \"Xfunc\"()" +
    "\r\n\t\twhile a==b" +
    "\r\n\t\t\tb=a" +
    "\r\n\t\t\ta=b" +
    "\r\n\t\twhile a==b" +
    "\r\n\t\t\ta=b" +
    "\r\n\t\twhile a==b" +
    "\r\n\t\t\treturn Name" +
    "\r\n\tnamedblock \"TESTBLOK\"()" +
    "\r\n\t\ta = \"True\""     
    , m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new
    );

            IEdge code16 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code16,
            "\"codeforech\"" +
    "\r\n\tfor vertex xxx in yyy\\kuery" +
    "\r\n\t\ty+=xxx" +
    "\r\n\t\tb+=xxx" 
    , m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new
    );

            IEdge code17 = code.AddVertexAndReturnEdge(null, null);
            MinusZero.Instance.DefaultFormalTextParser.Parse(code17,
            "\"parsetest\"" +
"\r\n\twhile \"3\" == \"4\"" +
"\r\n\t\tfunction \"Cin\"()" +
"\r\n\t\t@@\\Cin[]" +
"\r\n\tfunction \"B\"()" +
"\r\n\t\tfunction \"Bin\"()" +
"\r\n\t\t @@B\\Bin[]" +
"\r\n\tfunction \"A\"()" +
"\r\n\t\tfor vertex \"X\" in x" +
"\r\n\t\t\tfor vertex \"Y\" in y" +
"\r\n\t\t\t\tX += Y" +
"\r\n\twhile \"1\" == \"2\"" +
"\r\n\t\tfor vertex \"X\" in x" +
"\r\n\t\t\tfor vertex \"Y\" in y" +
"\r\n\t\t\t\tX += Y"
, m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new
    );

            IVertex pv = code.AddVertex(m0.MinusZero.Instance.root.Get(false, @"System\Meta\ZeroUML\Package"), "TestPackage");

            pv.AddEdge(m0.MinusZero.Instance.root.Get(false, @"System\Meta\Base\Vertex\$Is"),
                m0.MinusZero.Instance.root.Get(false, @"System\Meta\ZeroUML\Package"));

    }

        public static void CreateTestData()
        {            
            IVertex r = MinusZero.Instance.Root;

            //JsonSerializationStore jss = new JsonSerializationStore(@"c:\m0\x",MinusZero.Instance, new AccessLevelEnum[] { });

            //  IVertex tr = jss.Root;

            //MinusZero.Instance.Root.AddEdge(null, jss.Root);

            //return;

            IVertex tr = MinusZero.Instance.Root.AddVertex(null, "examples");

            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            queryTest(tr);

            exeTest(tr);

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(tr, r.Get(false, @"System\Meta"), @"{TEST3{Class:Customer{},Class:Person{$Description:opis,Attribute:Name,Attribute:Surname,Attribute:DateOfBirth},Class:Company{Attribute:Name,Attribute:RegistrationNumber,},Class:Adress{Attribute:Line 1,Attribute:Line 2,Attribute:Line 3,Attribute:City,Attribute:County,Attribute:Postal code,Attribute:Country},Class:Basket{Attribute:Creation date,Attribute:Status},Class:Item{Attribute:Name,Attribute:Description,Attribute:Price}}}");


            tr.Get(false, @"TEST3\Customer").AddEdge(r.Get(false, @"System\Meta?$Is"), r.Get(false, @"System\Meta\ZeroUML\Class"));
            tr.Get(false, @"TEST3\Person").AddEdge(r.Get(false, @"System\Meta?$Is"), r.Get(false, @"System\Meta\ZeroUML\Class"));
            tr.Get(false, @"TEST3\Company").AddEdge(r.Get(false, @"System\Meta?$Is"), r.Get(false, @"System\Meta\ZeroUML\Class"));
            tr.Get(false, @"TEST3\Adress").AddEdge(r.Get(false, @"System\Meta?$Is"), r.Get(false, @"System\Meta\ZeroUML\Class"));
            tr.Get(false, @"TEST3\Basket").AddEdge(r.Get(false, @"System\Meta?$Is"), r.Get(false, @"System\Meta\ZeroUML\Class"));
            tr.Get(false, @"TEST3\Item").AddEdge(r.Get(false, @"System\Meta?$Is"), r.Get(false, @"System\Meta\ZeroUML\Class"));

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(tr, r.Get(false, @"System\Meta"), "{TEST2,TEST22,TEST{Class:Person{Association:Spouse{$MaxCardinality:1,$MaxTargetCardinality:1},Aggregation:Child{$MaxCardinality:3},Attribute:Name,Attribute:Surname,Attribute:Age{MinValue:0,MaxValue:40},Attribute:NoseLength{MinValue:0,MaxValue:40},Attribute:Money{MinValue:0,MaxValue:1000},Attribute:IsGood,Attribute:IsPretty,Attribute:IsPretty2,Attribute:IsPretty3},Class:PersonB{Association:Spouse{$MaxCardinality:1,$MaxTargetCardinality:1},Aggregation:Child{$MaxCardinality:3},Attribute:Name,Attribute:Surname,Attribute:Age{MinValue:0,MaxValue:40},Attribute:NoseLength{MinValue:0,MaxValue:40},Attribute:Money{MinValue:0,MaxValue:1000},Attribute:IsGood,Attribute:IsPretty,Attribute:IsPretty2,Attribute:IsPretty3},Enum:Pretty{EnumValue:Yes,EnumValue:No,EnumValue:Maybe}}}");

            tr.Get(false, @"TEST\Pretty").AddEdge(r.Get(false, @"System\Meta?$Inherits"), r.Get(false, @"System\Meta\ZeroTypes\EnumBase"));
            tr.Get(false, @"TEST\Person").AddEdge(r.Get(false, @"System\Meta?$Is"), r.Get(false, @"System\Meta\ZeroUML\Class"));
            tr.Get(false, @"TEST\PersonB").AddEdge(r.Get(false, @"System\Meta?$Is"), r.Get(false, @"System\Meta\ZeroUML\Class"));

            tr.Get(false, @"TEST\Person\Spouse").AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$TargetQuery"), @"examples\TEST\Person:");

            ///




            ///

            IVertex smzt = r.Get(false, @"System\Meta\ZeroTypes");

            IVertex EdgeTarget = r.Get(false, @"System\Meta?$EdgeTarget");

            IVertex Person = tr.Get(false, @"TEST\Person");
            IVertex Person2 = tr.Get(false, @"TEST\PersonB");



            //////////////



            IVertex smu = r.Get(false, @"System\Meta\ZeroUML");
            IVertex smb = r.Get(false, @"System\Meta\Base");


            IVertex function_function = Person.AddVertex(smu.Get(false, @"Function"), "Sleep");

            function_function.AddEdge(smu.Get(false, @"Function\Output"), smzt.Get(false, "Integer"));

            IVertex ffi = function_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "time");

            ffi.AddEdge(smb.Get(false, @"Vertex\$VertexTarget"), smzt.Get(false, "Integer"));

            //
            ffi.AddVertex(smb.Get(false, @"Vertex\$Description"), "this is opis time");
            function_function.AddVertex(smb.Get(false, @"Vertex\$Description"), "this is opis function");
            //

            IVertex ffi2 = function_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "fastMode");

            ffi2.AddEdge(smb.Get(false, @"Vertex\$VertexTarget"), smzt.Get(false, "Boolean"));

            IVertex ffi3 = function_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "interrupt");

            ffi3.AddEdge(smb.Get(false, @"Vertex\$VertexTarget"), smzt.Get(false, "Integer"));


            function_function.AddVertex(smu.Get(false, @"[]"), null);

            function_function.AddEdge(smb.Get(false, @"$NewLine"), smb.Get(false, @"$Empty"));

            //



            Person.Get(false, "Name").AddEdge(EdgeTarget, smzt.Get(false, "String"));

            Person.Get(false, "Spouse").AddEdge(r.Get(false, @"System\Meta?$EdgeTarget"), Person);
            Person.Get(false, "Child").AddEdge(r.Get(false, @"System\Meta?$EdgeTarget"), Person);

            Person.Get(false, "Surname").AddEdge(EdgeTarget, smzt.Get(false, "String"));
            Person.Get(false, "Age").AddEdge(EdgeTarget, smzt.Get(false, "Integer"));
            Person.Get(false, "NoseLength").AddEdge(EdgeTarget, smzt.Get(false, "Float"));
            Person.Get(false, "Money").AddEdge(EdgeTarget, smzt.Get(false, "Decimal"));
            Person.Get(false, "IsGood").AddEdge(EdgeTarget, smzt.Get(false, "Boolean"));
            Person.Get(false, "IsPretty").AddEdge(EdgeTarget, tr.Get(false, @"TEST\Pretty"));
            Person.Get(false, "IsPretty2").AddEdge(EdgeTarget, tr.Get(false, @"TEST\Pretty"));
            Person.Get(false, "IsPretty3").AddEdge(EdgeTarget, tr.Get(false, @"TEST\Pretty"));

            Person2.Get(false, "Name").AddEdge(EdgeTarget, smzt.Get(false, "String"));

            Person2.Get(false, "Spouse").AddEdge(r.Get(false, @"System\Meta?$EdgeTarget"), Person2);
            Person2.Get(false, "Child").AddEdge(r.Get(false, @"System\Meta?$EdgeTarget"), Person2);

            Person2.Get(false, "Surname").AddEdge(EdgeTarget, smzt.Get(false, "String"));
            Person2.Get(false, "Age").AddEdge(EdgeTarget, smzt.Get(false, "Integer"));
            Person2.Get(false, "NoseLength").AddEdge(EdgeTarget, smzt.Get(false, "Float"));
            Person2.Get(false, "Money").AddEdge(EdgeTarget, smzt.Get(false, "Decimal"));
            Person2.Get(false, "IsGood").AddEdge(EdgeTarget, smzt.Get(false, "Boolean"));
            Person2.Get(false, "IsPretty").AddEdge(EdgeTarget, tr.Get(false, @"TEST\Pretty"));
            Person2.Get(false, "IsPretty2").AddEdge(EdgeTarget, tr.Get(false, @"TEST\Pretty"));
            Person2.Get(false, "IsPretty3").AddEdge(EdgeTarget, tr.Get(false, @"TEST\Pretty"));


            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(tr.Get(false, "TEST"), tr.Get(false, @"TEST"), "{Person:Person1{Name:Radek,Surname:Tereszczuk,Age:34,NoseLength:\"2,3\",Money:999,IsGood:False,IsPretty:},Person:Person2{Name:Maurycy,Surname:Tereszczuk,Age:1,NoseLength:1.1,Money:9999,IsGood:True,IsPretty:}}");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(tr.Get(false, "TEST"), tr.Get(false, @"TEST"), "{Person:Person3{Name:Radek,Surname:Tereszczuk,Age:34,NoseLength:\"2,3\",Money:999,IsGood:False,IsPretty:},Person:Person4{Name:Maurycy,Surname:Tereszczuk,Age:1,NoseLength:1.1,Money:9999,IsGood:True,IsPretty:}}");

            tr.Get(false, @"TEST\Person1").AddEdge(r.Get(false, @"System\Meta?$Is"), tr.Get(false, @"TEST\Person"));
            tr.Get(false, @"TEST\Person2").AddEdge(r.Get(false, @"System\Meta?$Is"), tr.Get(false, @"TEST\Person"));
            tr.Get(false, @"TEST\Person3").AddEdge(r.Get(false, @"System\Meta?$Is"), tr.Get(false, @"TEST\Person"));
            tr.Get(false, @"TEST\Person4").AddEdge(r.Get(false, @"System\Meta?$Is"), tr.Get(false, @"TEST\Person"));

            GraphUtil.ReplaceEdge(tr.Get(false, @"TEST\Person1"), "IsPretty", tr.Get(false, @"TEST\Pretty\No"));
            GraphUtil.ReplaceEdge(tr.Get(false, @"TEST\Person2"), "IsPretty", tr.Get(false, @"TEST\Pretty\Yes"));
            GraphUtil.ReplaceEdge(tr.Get(false, @"TEST\Person3"), "IsPretty", tr.Get(false, @"TEST\Pretty\Yes"));
            GraphUtil.ReplaceEdge(tr.Get(false, @"TEST\Person4"), "IsPretty", tr.Get(false, @"TEST\Pretty\Yes"));


            for (int x = 0; x < 20; x++)
            {
                m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(tr.Get(false, "TEST2"), tr.Get(false, @"TEST"), "{Person:Person1" + x + "{Name:Radek,Surname:Tereszczuk,Age:34,NoseLength:\"2,3\",Money:999,IsGood:False,IsPretty:},Person:Person2" + x + "{Name:Maurycy,Surname:Tereszczuk,Age:1,NoseLength:1.1,Money:9999,IsGood:True,IsPretty:}}");


                m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(tr.Get(false, "TEST2"), tr.Get(false, @"TEST"), "{Person:Person3" + x + "{Name:Magda,Surname:Tereszczuk,Age:18,NoseLength:\"2,1\",Money:999,IsGood:True,IsPretty:},Person:Person4" + x + "{Name:Jan,Surname:Kuciak,Age:10,NoseLength:0.6,Money:99999,IsGood:True,IsPretty:}}");

                GraphUtil.ReplaceEdge(tr.Get(false, @"TEST2\Person1" + x), "IsPretty", tr.Get(false, @"TEST\Pretty\No"));
                GraphUtil.ReplaceEdge(tr.Get(false, @"TEST2\Person2" + x), "IsPretty", tr.Get(false, @"TEST\Pretty\Yes"));
                GraphUtil.ReplaceEdge(tr.Get(false, @"TEST2\Person3" + x), "IsPretty", tr.Get(false, @"TEST\Pretty\Yes"));
                GraphUtil.ReplaceEdge(tr.Get(false, @"TEST2\Person4" + x), "IsPretty", tr.Get(false, @"TEST\Pretty\Yes"));

                tr.Get(false, @"TEST2\Person1" + x + @"\Radek").AddEdge(r.Get(false, @"System\Meta?$Is"), smzt.Get(false, "String"));


                tr.Get(false, @"TEST2\Person1" + x).AddEdge(r.Get(false, @"System\Meta?$Is"), tr.Get(false, @"TEST\Person"));
                tr.Get(false, @"TEST2\Person2" + x).AddEdge(r.Get(false, @"System\Meta?$Is"), tr.Get(false, @"TEST\Person"));
                tr.Get(false, @"TEST2\Person3" + x).AddEdge(r.Get(false, @"System\Meta?$Is"), tr.Get(false, @"TEST\Person"));
                tr.Get(false, @"TEST2\Person4" + x).AddEdge(r.Get(false, @"System\Meta?$Is"), tr.Get(false, @"TEST\Person"));                

            }

            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                {
                    tr.Get(false, @"TEST2\Person1" + x).AddEdge(tr.Get(false, @"TEST\Person\Child"), tr.Get(false, @"TEST2\Person1" + y));
                    tr.Get(false, @"TEST2\Person2" + x).AddEdge(tr.Get(false, @"TEST\Person\Child"), tr.Get(false, @"TEST2\Person2" + y));
                    tr.Get(false, @"TEST2\Person3" + x).AddEdge(tr.Get(false, @"TEST\Person\Child"), tr.Get(false, @"TEST2\Person3" + y));
                    tr.Get(false, @"TEST2\Person4" + x).AddEdge(tr.Get(false, @"TEST\Person\Child"), tr.Get(false, @"TEST2\Person4" + y));
                }

            for (int i = 1; i <= 100; i++)
            {
                IVertex x = tr.Get(false, "TEST22").AddVertex(null, i);

                for (int ii = 1; ii <= 1; ii++)
                {
                    IVertex xx = x.AddVertex(null, i + " " + ii);

                    for (int iii = 1; iii <= 1; iii++)
                    {
                        IVertex xxx = xx.AddVertex(null, i + " " + ii + " " + iii);

                        for (int iiii = 1; iiii <= 3; iiii++)
                            xxx.AddVertex(null, i + " " + ii + " " + iii + " " + iiii);
                    }
                }
            }

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(tr.Get(false, "TEST"), tr.Get(false, @"System\Meta"), "{Diagram:TestDiagram{Scale:100,SelectedEdges:,CreationPool:}}");

            tr.Get(false, @"TEST\TestDiagram").AddVertex(r.Get(false, @"System\Meta\Visualiser\Diagram\SizeX"), 600.0);

            tr.Get(false, @"TEST\TestDiagram").AddVertex(r.Get(false, @"System\Meta\Visualiser\Diagram\SizeY"), 600.0);

            tr.Get(false, @"TEST\TestDiagram").AddEdge(r.Get(false, @"System\Meta?$Is"), r.Get(false, @"System\Meta?Diagram"));

            IVertex i1 = tr.Get(false, @"TEST\TestDiagram").AddVertex(r.Get(false, @"System\Meta?Item"), null);

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(i1, r.Get(false, @"System\Meta"), "{PositionX:0,PositionY:0,SizeX:100,SizeY:100}");

            IVertex i2 = tr.Get(false, @"TEST\TestDiagram").AddVertex(r.Get(false, @"System\Meta?Item"), null);

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(i2, r.Get(false, @"System\Meta"), "{PositionX:200,PositionY:200,SizeX:100,SizeY:100}");

            i1.AddEdge(r.Get(false, @"System\Meta?$Is"), r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            i1.AddEdge(r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(false, @"System\Data\Visualiser\Diagram\Object"));

            EdgeHelper.AddEdgeVertexByToVertexByMeta(i1, r.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), tr.Get(false, @"TEST\Person1"));

            i2.AddEdge(r.Get(false, @"System\Meta?$Is"), r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            i2.AddEdge(r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(false, @"System\Data\Visualiser\Diagram\Object"));

            EdgeHelper.AddEdgeVertexByToVertexByMeta(i2, r.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), tr.Get(false, @"TEST\Person2"));





            i1 = tr.Get(false, @"TEST\TestDiagram").AddVertex(r.Get(false, @"System\Meta?Item"), null);

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(i1, r.Get(false, @"System\Meta"), "{PositionX:350,PositionY:0}");

            i2 = tr.Get(false, @"TEST\TestDiagram").AddVertex(r.Get(false, @"System\Meta?Item"), null);

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(i2, r.Get(false, @"System\Meta"), "{PositionX:0,PositionY:350}");

            i1.AddEdge(r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(false, @"System\Data\Visualiser\Diagram\Object"));

            i1.AddEdge(r.Get(false, @"System\Meta?$Is"), r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            EdgeHelper.AddEdgeVertexByToVertexByMeta(i1, r.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), tr.Get(false, @"TEST\Person3"));

            i2.AddEdge(r.Get(false, @"System\Meta?$Is"), r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            i2.AddEdge(r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(false, @"System\Data\Visualiser\Diagram\Object"));

            EdgeHelper.AddEdgeVertexByToVertexByMeta(i2, r.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), tr.Get(false, @"TEST\Person4"));

            /////////////////////

            /* GeneralUtil.ParseAndExcute(r.Get(false, "TEST"), r.Get(false, @"System\Meta"), "{Class:X1,Class:X2,Class:X3,Class:X4,Class:PersonA,Class:PersonB,Class:PersonB2{Attribute:New}}");

             r.Get(false, @"TEST\PersonB2\New").AddEdge(r.Get(false, @"System\Meta*$EdgeTarget"), r.Get(false, @"System\Meta*String"));

             VertexOperations.AddInstance(r.Get(false, "TEST"), r.Get(false, @"TEST\PersonB2"), r.Get(false, @"TEST\Person")).Value="XXX";

             r.Get(false, @"TEST\X2").AddEdge(r.Get(false, @"System\Meta*$Inherits"), r.Get(false, @"TEST\Person"));
             r.Get(false, @"TEST\X3").AddEdge(r.Get(false, @"System\Meta*$Inherits"), r.Get(false, @"TEST\X2"));
             r.Get(false, @"TEST\X4").AddEdge(r.Get(false, @"System\Meta*$Inherits"), r.Get(false, @"TEST\X3"));

             r.Get(false, @"TEST\PersonA").AddEdge(r.Get(false, @"System\Meta*$Inherits"), r.Get(false, @"TEST\Person"));
             r.Get(false, @"TEST\PersonB").AddEdge(r.Get(false, @"System\Meta*$Inherits"), r.Get(false, @"TEST\Person"));
             r.Get(false, @"TEST\PersonB2").AddEdge(r.Get(false, @"System\Meta*$Inherits"), r.Get(false, @"TEST\PersonB"));

             r.Get(false, @"TEST\XXX").AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"TEST\PersonA"));
             r.Get(false, @"TEST\XXX").AddEdge(r.Get(false, @"System\Meta*$Is"), r.Get(false, @"TEST\X4"));*/

            //////////////////////

            IVertex tt = tr.Get(false, "TEST").AddVertex(r.Get(false, "System\\Meta\\ZeroUML\\Class"), "TestClass");

            for (int x = 0; x < 1; x++)
                for (int y = 0; y < 1; y++)
                {
                    IVertex tta = tt.AddVertex(r.Get(false, "System\\Meta\\ZeroUML\\Class\\Attribute"), "a" + x + " " + y + ";" + randomChars());
                    tta.AddVertex(r.Get(false, "System\\Meta\\Base\\Vertex\\$Group"), x.ToString());
                    tta.AddVertex(r.Get(false, "System\\Meta\\Base\\Vertex\\$Section"), y.ToString());

                    tta.AddEdge(r.Get(false, "System\\Meta\\Base\\Vertex\\$EdgeTarget"), r.Get(false, "System\\Meta\\ZeroTypes\\String"));

                    IVertex ttb = tt.AddVertex(r.Get(false, "System\\Meta\\ZeroUML\\Class\\Attribute"), "b" + x + " " + y + ";" + randomChars());
                    ttb.AddVertex(r.Get(false, "System\\Meta\\Base\\Vertex\\$Group"), x.ToString());
                    //ttb.AddVertex(r.Get(false, "System*$Section"), y.ToString());
                    ttb.AddEdge(r.Get(false, "System\\Meta\\Base\\Vertex\\$EdgeTarget"), r.Get(false, "System\\Meta\\ZeroTypes\\String"));

                    IVertex ttc = tt.AddVertex(r.Get(false, "System\\Meta\\ZeroUML\\Class\\Attribute"), "c" + x + " " + y + ";" + randomChars());
                    ttc.AddVertex(r.Get(false, "System\\Meta\\Base\\Vertex\\$Group"), x.ToString());
                    ttc.AddVertex(r.Get(false, "System\\Meta\\Base\\Vertex\\$Section"), y.ToString());
                    ttc.AddVertex(r.Get(false, "System\\Meta\\Base\\Vertex\\$MaxCardinality"), 6);
                    ttc.AddEdge(r.Get(false, "System\\Meta\\Base\\Vertex\\$EdgeTarget"), r.Get(false, "System\\Meta\\ZeroTypes\\String"));
                }

            VertexOperations.AddInstance(tr.Get(false, "TEST"), tt);

            //////////////////////


            IVertex start = tr.Get(false, @"TEST3");

            for (int i = 0; i < 1; i++)
            {
                IVertex sm = start.AddVertex(r.Get(false, @"System\Meta\ZeroUML\StateMachine"), "sm " + i);

                for (int ii = 0; ii < 1; ii++)
                    sm.AddVertex(r.Get(false, @"System\Meta\ZeroUML\StateMachine\State"), "state " + ii + " of machine" + i);

                IVertex allstates = sm.GetAll(false, "");

                foreach (IEdge e in allstates)
                    foreach (IEdge ee in allstates)
                        e.To.AddEdge(r.Get(false, @"System\Meta\ZeroUML\StateMachine\State\Transition"), ee.To);
            }

            //////////////////////

            IVertex associations = tr.GetAll(false, @"TEST\Person\Association:");
            IVertex ismeta = r.Get(false, @"System\Meta?$Is");
            IVertex asmeta = r.Get(false, @"System\Meta\ZeroUML\Class\Association");

            //foreach (IEdge v in associations)
            //   v.To.AddEdge(ismeta, asmeta);

            IVertex attributes = tr.GetAll(false, @"TEST\Person\Attribute:");
            //IVertex ismeta=r.Get(false, @"System\Meta*$Is");
            IVertex ameta = r.Get(false, @"System\Meta\ZeroUML\Class\Attribute");

            foreach (IEdge v in attributes)
                v.To.AddEdge(ismeta, ameta);

            attributes = tr.GetAll(false, @"TEST3\\Attribute:");

            foreach (IEdge v in attributes)
                v.To.AddEdge(ismeta, ameta);

            IVertex test = tr.Get(false, "TEST");

            test.AddVertex(test.AddVertex(null, "Counter"), (int)0);


            IVertex vvv = VertexOperations.AddInstance(test, r.Get(false, @"System\Meta\Base\$Import"));

            vvv.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$IsLink"), MinusZero.Instance.Empty);

            vvv.Value = "tst";

            test.AddEdge(tr.Get(false, @"TEST\tst"), r.Get(false, @"System\Meta\Visualiser"));

            /////

            IVertex aattributes = tr.GetAll(false, @"TEST\\Attribute:");

            IVertex isAggregation = r.Get(false, @"System\Meta\Base\Vertex\$IsAggregation");
            IVertex empty = r.Get(false, @"System\Meta\Base\$Empty");

            foreach (IEdge v in aattributes)
                v.To.AddEdge(isAggregation, empty);

            IVertex aggregations = tr.GetAll(false, @"TEST\\Aggregation:");

            foreach (IEdge v in aggregations)
                v.To.AddEdge(isAggregation, empty);

            ///

            IVertex vx = tr.AddVertex(null, "X");

            IVertex my = vx.AddVertex(null, "j e s ");

            IVertex vxx = vx.AddVertex(null, "VXX");

            IVertex c = VertexOperations.AddInstance(vxx, smu.Get(false, "FunctionCall"));

            c.Value = "";

            c.AddVertex(smu.Get(false, @"MultiOperator\Expression"), "raz");

            IVertex dwa = c.AddVertex(smu.Get(false, @"MultiOperator\Expression"), "dwa");

            //dwa.AddVertex(smb.Get(false, @"Vertex\$Description"), "3 3 3");

            c.AddEdge(smu.Get(false, @"MultiOperator\Expression"), my);

            IVertex cztery = c.AddVertex(smu.Get(false, @"MultiOperator\Expression"), "cztery");

            //cztery.AddVertex(smb.Get(false, @"Vertex\$Description"), "cztery 1");

            //cztery.AddVertex(smb.Get(false, @"Vertex\$Description"), "cztery 2");


            // IVertex zzz = vx.AddVertex(null, "raz");

            //   IVertex yyy = vx.AddVertex(null, "dwa");

            //  IVertex wh = VertexOperations.AddInstance(vx, smu.Get(false, @"While"));


            // IVertex plus = VertexOperations.AddInstance(wh, smu.Get(false, "-"), smu.Get(false, @"While\Test"));

            //IVertex plus = VertexOperations.AddInstance(c, smu.Get(false, "+"), smu.Get(false, @"MultiOperator\Expression"));

            //plus.AddVertex(smb.Get(false, @"Vertex\$Description"), "plus opis MAIN");

            //plus.AddVertex(null, "KUPA");

            //  plus.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "10");

            // plus.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "30");

            //plus.Value = "";

            // IVertex h = vx.AddVertex(null, "h");

            //  vx.AddEdge(zzz,plus);

            //  vx.AddVertex(zzz, "");

            // vx.AddEdge(yyy, plus);

            /*IVertex leftplus = VertexOperations.AddInstance(plus, smu.Get(false, "-"), smu.Get(false, @"DoubleOperator\LeftExpression"));

          leftplus.Value = "";

            leftplus.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "1");

            leftplus.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "2");

           IVertex rightplus = VertexOperations.AddInstance(plus, smu.Get(false, "+"), smu.Get(false, @"DoubleOperator\RightExpression"));

          //rightplus.AddVertex(smb.Get(false, @"Vertex\$Description"), "plus opis tak");

          rightplus.Value = "";

          IVertex w3=rightplus.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "3");
         // w3.AddVertex(smb.Get(false, @"Vertex\$Description"), "3 3 3");


          IVertex v4=rightplus.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "4");

            v4.AddVertex(smb.Get(false, @"Vertex\$Description"), "opis");


          */


            //////////////////////////


            IVertex xXx = tr.AddVertex(null, "XX");


            addf(xXx);


            //////////////////////////

            IVertex yv = tr.AddVertex(null, "Y");

            /////////////////

            // jss.Detach();
            //jss.CommitTransaction();
            // jss.Attach();

        }

        static void addf(IVertex where)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex smzt = r.Get(false, @"System\Meta\ZeroTypes");
            IVertex smu = r.Get(false, @"System\Meta\ZeroUML");
            IVertex smb = r.Get(false, @"System\Meta\Base");

            IVertex function_function = where.AddVertex(smu.Get(false, @"Function"), "Sleep");

            function_function.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "Function"));

            function_function.AddEdge(smu.Get(false, @"Function\Output"), smzt.Get(false, "Integer"));

            IVertex ffi = function_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "time");

            ffi.AddEdge(smb.Get(false, @"Vertex\$VertexTarget"), smzt.Get(false, "Integer"));

            //
            ffi.AddVertex(smb.Get(false, @"Vertex\$Description"), "this is opis");

            //function_function.AddVertex(smb.Get(false, @"Vertex\$Description"), "this is opis");
            //

            IVertex ffi2 = function_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "fastMode");

            ffi2.AddEdge(smb.Get(false, @"Vertex\$VertexTarget"), smzt.Get(false, "Boolean"));

            IVertex ffi3 = function_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "interrupt");

            ffi3.AddEdge(smb.Get(false, @"Vertex\$VertexTarget"), smzt.Get(false, "Integer"));


            ffi3.AddVertex(smb.Get(false, @"Vertex\$Description"), "inter opis");


            function_function.AddVertex(smu.Get(false, @"[]"), null);

            function_function.AddEdge(smb.Get(false, @"$NewLine"), smb.Get(false, @"$Empty"));
        }
    }
}
