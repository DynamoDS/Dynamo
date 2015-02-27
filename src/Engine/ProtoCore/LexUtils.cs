using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace ProtoCore.Utils
{
    public static class LexerUtils
    {
        //@TODO(Luke, Aysuh, Jun): This needs completely replacing with a better parser architecture
        public static string HashAngleReplace(string source)
        {
            List<string> keywords = new List<string>();
            keywords.Add("Associative");
            keywords.Add("Imperative");
            keywords.Add("options");

            List<string> options = new List<string>();
            options.Add("fingerprint");
            options.Add("version");

            StringBuilder outputBuffer = new StringBuilder();
            char token;
            List<int> curlyCounterStack = new List<int>();
            List<bool> inLangBlock = new List<bool>();
            curlyCounterStack.Add(0);

            using (StringReader reader = new StringReader(source))
            {
                while (reader.Peek() >= 0)
                {
                    token = (char)reader.Read();

                    if (token == '/' && ((char)reader.Peek() == '/' || (char)reader.Peek() == '*'))
                    {
                        //comment begins here                        
                        outputBuffer.Append(token);
                        token = (char)reader.Read();
                        outputBuffer.Append(token);

                        if (token == '/')                       //single line comment starts here
                        {
                            token = (char)reader.Read();

                            while ((int)token != 10 && (int)token != 65535)            //ASCII 10 = '\n'. 
                            {
                                outputBuffer.Append(token);
                                token = (char)reader.Read();
                            }

                            outputBuffer.Append('\n'.ToString());
                        }
                        else if (token == '*')
                        {
                            token = (char)reader.Read();

                            while (!((token == '*' && reader.Peek() == 47) || (int)token == 65535)) //ASCII 47 = '/'
                            {
                                outputBuffer.Append(token);
                                token = (char)reader.Read();
                            }

                            outputBuffer.Append("*/");
                            reader.Read();  //throw away the closing '/' of the multi-line comment since it
                            // was manually entered in the above line
                        }
                    }
                    else if (token == '[')
                    {
                        StringBuilder keyword = new StringBuilder();
                        StringBuilder fingerprint = new StringBuilder();
                        outputBuffer.Append(token);
                        char la = (char)reader.Peek(); //lookahead token
                        token = (char)reader.Read();
                        bool keywordCompleted = false;

                        while (token != ']')
                        {

                            while (la == ' ' || la == '\t')
                            {
                                la = (char)reader.Peek();
                                token = (char)reader.Read();
                            }

                            /*
                            if (la != 'i' && la != 'a')
                            {
                                throw new Exception("keyword exception");
                            }
                            */

                            if (token != '"')
                                keyword.Append(token);
                            outputBuffer.Append(token);

                            if (keywords.Contains(keyword.ToString().ToLower().Trim()))
                                keywordCompleted = true;

                            token = (char)reader.Read();

                            if (keywordCompleted)
                            {
                                StringBuilder option = new StringBuilder();
                                StringBuilder value = new StringBuilder();
                                if (token == ' ' || token == '\t' || token == ']')
                                    continue;
                                else if (token == ',') //expect Fingerprint or Version
                                {
                                    outputBuffer.Append(token);

                                    while (true)
                                    {
                                        if ((char)reader.Peek() == ' ' || (char)reader.Peek() == '\t')
                                        {
                                            token = (char)reader.Read();
                                            outputBuffer.Append(token);
                                        }
                                        else if ((char)reader.Peek() == 'f' || (char)reader.Peek() == 'v')
                                            break;
                                        else
                                            throw new Exception("keyword exception");
                                    }
                                    while ((char)reader.Peek() != '"')
                                    {
                                        token = (char)reader.Read();
                                        option.Append(token);
                                        outputBuffer.Append(token);
                                    }

                                    if (options.Contains(option.ToString().Replace("=", "").ToLower().Trim()))
                                    {
                                        token = (char)reader.Read();  //handle the opening\'
                                        outputBuffer.Append(token);
                                        token = (char)reader.Read();
                                        //outputBuffer.Append(token);

                                        while (token != '"')
                                        {
                                            value.Append(token);
                                            outputBuffer.Append(token);
                                            token = (char)reader.Read();

                                        }
                                    }
                                    else
                                        throw new Exception("keyword exception");


                                }
                                else
                                    throw new Exception("keyword exception");
                            }
                        }
                        if (keywords.Contains(keyword.ToString().ToLower().Trim()))
                        {
                            do
                            {
                                outputBuffer.Append(token);
                                token = (char)reader.Read();
                            } while (token == '\r' || token == '\n' || token == ' ' || token == '\t');

                            if (token != '{')
                                throw new Exception("Language block opening parenthesis expected.");
                           
                            curlyCounterStack.Add(1);
                            outputBuffer.Append("<#");
                            inLangBlock.Add(true);
                        }
                        else
                        {
                            outputBuffer.Append(token);
                        }
                    }
                    else
                    {
                        if (token == '{')
                            curlyCounterStack[curlyCounterStack.Count - 1]++;
                        else if (token == '}')
                            curlyCounterStack[curlyCounterStack.Count - 1]--;

                        if (    token == '}' 
                            &&  curlyCounterStack[curlyCounterStack.Count - 1] == 0
                            && inLangBlock.Count != 0
                            && inLangBlock[inLangBlock.Count -1])
                        {
                            outputBuffer.Append("#>");
                            curlyCounterStack.RemoveAt(curlyCounterStack.Count - 1);
                            inLangBlock.RemoveAt(inLangBlock.Count - 1);
                        }
                        else
                            outputBuffer.Append(token);
                    }

                }
            }

            return outputBuffer.ToString();
        }
    }

    public static class StringUtils
    {
        public static string streamToString(System.IO.Stream stream)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(stream);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            string str = reader.ReadToEnd();
            return str;
        }
    }
}

