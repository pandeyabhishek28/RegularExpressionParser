using RegularExpressionParser.Model;
using RegularExpressionParser.Utils;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace RegularExpressionParser.BusinessLogic
{

    public class RegularExpressionRecognizer
    {
        static private ExpressionValidator _expressionValidator = new ExpressionValidator();

        private int _lastErrorIndex = -1;
        private int _lastErrorLength = -1;
        private CompilationStatus _lastCompilationStatus = CompilationStatus.SUCCESS;
        private bool _matchAtStart = false;
        private bool _matchAtEnd = false;
        private bool _useGreedy = true;
        private TransitionState _startingDFAState = null;

        public bool UseGreedy
        {
            get
            {
                return _useGreedy;
            }
            set
            {
                _useGreedy = value;
            }
        }

        public RegularExpressionRecognizer()
        {

        }

        private string ConvertToPostfix(string sInfixPattern)
        {
            Stack stackOperator = new Stack();
            Queue queuePostfix = new Queue();
            bool escape = false;

            for (int i = 0; i < sInfixPattern.Length; i++)
            {
                char ch = sInfixPattern[i];

                if (escape == false && ch == MetaSymbol.ESCAPE)
                {
                    queuePostfix.Enqueue(ch);
                    escape = true;
                    continue;
                }

                if (escape == true)
                {
                    queuePostfix.Enqueue(ch);
                    escape = false;
                    continue;
                }
                switch (ch)
                {
                    case MetaSymbol.OPEN_PREN:
                        stackOperator.Push(ch);
                        break;
                    case MetaSymbol.CLOSE_PREN:
                        while ((char)stackOperator.Peek() != MetaSymbol.OPEN_PREN)
                        {
                            queuePostfix.Enqueue(stackOperator.Pop());
                        }
                        stackOperator.Pop();
                        break;
                    default:
                        while (stackOperator.Count > 0)
                        {
                            char chPeeked = (char)stackOperator.Peek();

                            int nPriorityPeek = GetOperatorPriority(chPeeked);
                            int nPriorityCurr = GetOperatorPriority(ch);

                            if (nPriorityPeek >= nPriorityCurr)
                            {
                                queuePostfix.Enqueue(stackOperator.Pop());
                            }
                            else
                            {
                                break;
                            }
                        }
                        stackOperator.Push(ch);
                        break;
                }
            }

            while (stackOperator.Count > 0)
            {
                queuePostfix.Enqueue((char)stackOperator.Pop());
            }
            StringBuilder sb = new StringBuilder(1024);
            while (queuePostfix.Count > 0)
            {
                sb.Append((char)queuePostfix.Dequeue());
            }

            return sb.ToString();
        }

        private int GetOperatorPriority(char argCharSymbol)
        {
            switch (argCharSymbol)
            {
                case MetaSymbol.OPEN_PREN:
                    return 0;
                case MetaSymbol.ALTERNATE:
                    return 1;
                case MetaSymbol.CONCANATE:
                    return 2;
                case MetaSymbol.ZERO_OR_ONE:
                case MetaSymbol.ZERO_OR_MORE:
                case MetaSymbol.ONE_OR_MORE:
                    return 3;
                case MetaSymbol.COMPLEMENT:
                    return 4;
                default:
                    return 5;
            }
        }

        public CompilationStatus CompileWithStats(string argPattern, StringBuilder argSBStats)
        {
            if (argSBStats == null)
            {
                return Compile(argPattern);
            }

            TransitionState.ResetCounter();

            int lineLength = 0;

            ValidationInfo vi = _expressionValidator.Validate(argPattern);

            UpdateValidationInfo(vi);

            if (vi.ErrorCode != CompilationStatus.SUCCESS)
            {
                return vi.ErrorCode;
            }

            string regExpressionPostfix = ConvertToPostfix(vi.FormattedString);

            argSBStats.AppendLine("Original pattern:\t\t" + argPattern);
            argSBStats.AppendLine("Pattern after formatting:\t" + vi.FormattedString);
            argSBStats.AppendLine("Pattern after postfix:\t\t" + regExpressionPostfix);
            argSBStats.AppendLine();

            TransitionState stateStartNfa = CreateNfa(regExpressionPostfix);
            argSBStats.AppendLine();
            argSBStats.AppendLine("NFA Table:");
            lineLength = GetSerializedFsa(stateStartNfa, argSBStats);
            argSBStats.AppendFormat(("").PadRight(lineLength, '*'));
            argSBStats.AppendLine();

            TransitionState.ResetCounter();
            TransitionState stateStartDfa = ConvertToDfa(stateStartNfa);
            argSBStats.AppendLine();
            argSBStats.AppendLine("DFA Table:");
            lineLength = GetSerializedFsa(stateStartDfa, argSBStats);
            argSBStats.AppendFormat(("").PadRight(lineLength, '*'));
            argSBStats.AppendLine();

            TransitionState stateStartDfaM = ReduceDfa(stateStartDfa);
            _startingDFAState = stateStartDfaM;
            argSBStats.AppendLine();
            argSBStats.AppendLine("DFA M' Table:");
            lineLength = GetSerializedFsa(stateStartDfaM, argSBStats);
            argSBStats.AppendFormat(("").PadRight(lineLength, '*'));
            argSBStats.AppendLine();

            return CompilationStatus.SUCCESS;
        }

        public CompilationStatus Compile(string sPattern)
        {
            var vaidationInfo = _expressionValidator.Validate(sPattern);

            UpdateValidationInfo(vaidationInfo);

            if (vaidationInfo.ErrorCode != CompilationStatus.SUCCESS)
            {
                return vaidationInfo.ErrorCode;
            }

            TransitionState.ResetCounter();
            string sRegExConcat = vaidationInfo.FormattedString;

            string sRegExPostfix = ConvertToPostfix(sRegExConcat);

            TransitionState stateStartNfa = CreateNfa(sRegExPostfix);

            TransitionState.ResetCounter();
            TransitionState stateStartDfa = ConvertToDfa(stateStartNfa);
            _startingDFAState = stateStartDfa;

            _startingDFAState = ReduceDfa(stateStartDfa);

            return CompilationStatus.SUCCESS;
        }

        private Hashset Eclosure(TransitionState stateStart)
        {
            Hashset setProcessed = new Hashset();
            Hashset setUnprocessed = new Hashset();

            setUnprocessed.AddElement(stateStart);

            while (setUnprocessed.Count > 0)
            {
                TransitionState state = (TransitionState)setUnprocessed[0];
                TransitionState[] arrTrans = state.GetTransitions(MetaSymbol.EPSILON);
                setProcessed.AddElement(state);
                setUnprocessed.RemoveElement(state);

                if (arrTrans != null)
                {
                    foreach (TransitionState stateEpsilon in arrTrans)
                    {
                        if (!setProcessed.ElementExist(stateEpsilon))
                        {
                            setUnprocessed.AddElement(stateEpsilon);
                        }
                    }
                }
            }

            return setProcessed;
        }

        private Hashset Eclosure(Hashset setState)
        {
            var setAllEclosure = new Hashset();
            TransitionState state;
            foreach (object obj in setState)
            {
                state = obj as TransitionState;
                var setEclosure = Eclosure(state);
                setAllEclosure.Union(setEclosure);
            }
            return setAllEclosure;
        }

        private Hashset Move(Hashset setState, string sInputSymbol)
        {
            var set = new Hashset();
            TransitionState state;
            foreach (object obj in setState)
            {
                state = obj as TransitionState;
                Hashset setMove = Move(state, sInputSymbol);
                set.Union(setMove);
            }
            return set;
        }

        private Hashset Move(TransitionState state, string sInputSymbol)
        {
            var set = new Hashset();

            var arrTrans = state.GetTransitions(sInputSymbol);

            if (arrTrans != null)
            {
                set.AddElementRange(arrTrans);
            }

            return set;
        }

        private TransitionState CreateNfa(string argExpressionPostfix)
        {
            var stackNFA = new Stack();
            NFAExpression expression = null;
            NFAExpression expressionA = null;
            NFAExpression expressionB = null;
            NFAExpression newExpression = null;
            bool escape = false;

            foreach (char postfixChar in argExpressionPostfix)
            {
                if (escape == false && postfixChar == MetaSymbol.ESCAPE)
                {
                    escape = true;
                    continue;
                }

                if (escape == true)
                {
                    newExpression = new NFAExpression();
                    newExpression.StartState().AddTransition(postfixChar.ToString(), newExpression.FinalState());

                    stackNFA.Push(newExpression);
                    escape = false;
                    continue;
                }

                switch (postfixChar)
                {
                    case MetaSymbol.ZERO_OR_MORE:
                        expressionA = (NFAExpression)stackNFA.Pop();
                        newExpression = new NFAExpression();

                        expressionA.FinalState().AddTransition(MetaSymbol.EPSILON, expressionA.StartState());
                        expressionA.FinalState().AddTransition(MetaSymbol.EPSILON, newExpression.FinalState());

                        newExpression.StartState().AddTransition(MetaSymbol.EPSILON, expressionA.StartState());
                        newExpression.StartState().AddTransition(MetaSymbol.EPSILON, newExpression.FinalState());
                        stackNFA.Push(newExpression);
                        break;
                    case MetaSymbol.ALTERNATE:
                        expressionB = (NFAExpression)stackNFA.Pop();
                        expressionA = (NFAExpression)stackNFA.Pop();
                        newExpression = new NFAExpression();

                        expressionA.FinalState().AddTransition(MetaSymbol.EPSILON, newExpression.FinalState());
                        expressionB.FinalState().AddTransition(MetaSymbol.EPSILON, newExpression.FinalState());

                        newExpression.StartState().AddTransition(MetaSymbol.EPSILON, expressionA.StartState());
                        newExpression.StartState().AddTransition(MetaSymbol.EPSILON, expressionB.StartState());
                        stackNFA.Push(newExpression);
                        break;
                    case MetaSymbol.CONCANATE:
                        expressionB = (NFAExpression)stackNFA.Pop();
                        expressionA = (NFAExpression)stackNFA.Pop();
                        expressionA.FinalState().AddTransition(MetaSymbol.EPSILON, expressionB.StartState());

                        newExpression = new NFAExpression(expressionA.StartState(), expressionB.FinalState());
                        stackNFA.Push(newExpression);
                        break;
                    case MetaSymbol.ONE_OR_MORE:
                        expressionA = (NFAExpression)stackNFA.Pop();
                        newExpression = new NFAExpression();

                        newExpression.StartState().AddTransition(MetaSymbol.EPSILON, expressionA.StartState());
                        newExpression.FinalState().AddTransition(MetaSymbol.EPSILON, expressionA.StartState());
                        expressionA.FinalState().AddTransition(MetaSymbol.EPSILON, newExpression.FinalState());
                        stackNFA.Push(newExpression);
                        break;
                    case MetaSymbol.ZERO_OR_ONE:
                        expressionA = (NFAExpression)stackNFA.Pop();
                        newExpression = new NFAExpression();

                        newExpression.StartState().AddTransition(MetaSymbol.EPSILON, expressionA.StartState());
                        newExpression.StartState().AddTransition(MetaSymbol.EPSILON, newExpression.FinalState());
                        expressionA.FinalState().AddTransition(MetaSymbol.EPSILON, newExpression.FinalState());
                        stackNFA.Push(newExpression);
                        break;
                    case MetaSymbol.ANY_ONE_CHAR:
                        newExpression = new NFAExpression();
                        newExpression.StartState().AddTransition(MetaSymbol.ANY_ONE_CHAR_TRANS, newExpression.FinalState());
                        stackNFA.Push(newExpression);
                        break;
                    case MetaSymbol.COMPLEMENT:
                        expressionA = (NFAExpression)stackNFA.Pop();

                        NFAExpression exprDummy = new NFAExpression();
                        exprDummy.StartState().AddTransition(MetaSymbol.DUMMY, exprDummy.FinalState());

                        expressionA.FinalState().AddTransition(MetaSymbol.EPSILON, exprDummy.StartState());

                        NFAExpression exprAny = new NFAExpression();
                        exprAny.StartState().AddTransition(MetaSymbol.ANY_ONE_CHAR_TRANS, exprAny.FinalState());

                        newExpression = new NFAExpression();
                        newExpression.StartState().AddTransition(MetaSymbol.EPSILON, expressionA.StartState());
                        newExpression.StartState().AddTransition(MetaSymbol.EPSILON, exprAny.StartState());

                        exprAny.FinalState().AddTransition(MetaSymbol.EPSILON, newExpression.FinalState());
                        exprDummy.FinalState().AddTransition(MetaSymbol.EPSILON, newExpression.FinalState());

                        stackNFA.Push(newExpression);
                        break;
                    default:
                        newExpression = new NFAExpression();
                        newExpression.StartState().AddTransition(postfixChar.ToString(), newExpression.FinalState());
                        stackNFA.Push(newExpression);
                        break;
                }
            }

            expression = (NFAExpression)stackNFA.Pop();
            expression.FinalState().AcceptingState = true;

            return expression.StartState();
        }

        private TransitionState ConvertToDfa(TransitionState stateStartNfa)
        {
            Hashset setAllInput = new Hashset();
            Hashset setAllState = new Hashset();

            GetAllStateAndInput(stateStartNfa, setAllState, setAllInput);
            setAllInput.RemoveElement(MetaSymbol.EPSILON);

            NFAToDFAHelper helper = new NFAToDFAHelper();
            Hashset setMove = null;
            Hashset setEclosure = null;

            setEclosure = Eclosure(stateStartNfa);
            TransitionState stateStartDfa = new TransitionState();

            if (IsAcceptingGroup(setEclosure) == true)
            {
                stateStartDfa.AcceptingState = true;
            }

            helper.AddDfaState(stateStartDfa, setEclosure);

            string sInputSymbol = String.Empty;

            TransitionState stateT = null;
            Hashset setT = null;
            TransitionState stateU = null;

            while ((stateT = helper.GetNextUnmarkedDfaState()) != null)
            {
                helper.MarkVisited(stateT);

                setT = helper.GetEclosureByDfaState(stateT);

                foreach (object obj in setAllInput)
                {
                    sInputSymbol = obj.ToString();

                    setMove = Move(setT, sInputSymbol);

                    if (setMove.IsEmpty() == false)
                    {
                        setEclosure = Eclosure(setMove);

                        stateU = helper.FindDfaStateByEclosure(setEclosure);

                        if (stateU == null)
                        {
                            stateU = new TransitionState();
                            if (IsAcceptingGroup(setEclosure) == true)
                            {
                                stateU.AcceptingState = true;
                            }

                            helper.AddDfaState(stateU, setEclosure);
                        }

                        stateT.AddTransition(sInputSymbol, stateU);
                    }
                }
            }

            return stateStartDfa;
        }

        private TransitionState ReduceDfa(TransitionState stateStartDfa)
        {
            Hashset setInputSymbol = new Hashset();
            Hashset setAllDfaState = new Hashset();

            GetAllStateAndInput(stateStartDfa, setAllDfaState, setInputSymbol);

            TransitionState stateStartReducedDfa = null;
            ArrayList arrGroup = null;

            arrGroup = PartitionDfaGroups(setAllDfaState, setInputSymbol);

            foreach (object objGroup in arrGroup)
            {
                Hashset setGroup = (Hashset)objGroup;

                bool bAcceptingGroup = IsAcceptingGroup(setGroup);
                bool bStartingGroup = setGroup.ElementExist(stateStartDfa);

                TransitionState stateRepresentative = (TransitionState)setGroup[0];

                if (bStartingGroup == true)
                {
                    stateStartReducedDfa = stateRepresentative;
                }

                if (bAcceptingGroup == true)
                {
                    stateRepresentative.AcceptingState = true;
                }

                if (setGroup.GetCardinality() == 1)
                {
                    continue;
                }

                setGroup.RemoveElement(stateRepresentative);

                TransitionState stateToBeReplaced = null;
                int nReplecementCount = 0;
                foreach (object objStateToReplaced in setGroup)
                {
                    stateToBeReplaced = (TransitionState)objStateToReplaced;

                    setAllDfaState.RemoveElement(stateToBeReplaced);

                    foreach (object objState in setAllDfaState)
                    {
                        TransitionState state = (TransitionState)objState;
                        nReplecementCount += state.ReplaceTransitionState(stateToBeReplaced, stateRepresentative);
                    }
                }
            }

            int nIndex = 0;
            while (nIndex < setAllDfaState.Count)
            {
                TransitionState state = (TransitionState)setAllDfaState[nIndex];
                if (state.IsDeadState())
                {
                    setAllDfaState.RemoveAt(nIndex);
                    continue;
                }
                nIndex++;
            }

            return stateStartReducedDfa;
        }

        private bool IsAcceptingGroup(Hashset setGroup)
        {
            TransitionState state;

            foreach (object objState in setGroup)
            {
                state = (TransitionState)objState;

                if (state.AcceptingState == true)
                {
                    return true;
                }
            }

            return false;
        }

        private ArrayList PartitionDfaGroups(Hashset setMasterDfa, Hashset setInputSymbol)
        {
            ArrayList arrGroup = new ArrayList();
            HashMap hashMap = new HashMap();
            Hashset emptySet = new Hashset();
            Hashset acceptingSet = new Hashset();
            Hashset nonAcceptingSet = new Hashset();

            foreach (object objState in setMasterDfa)
            {
                TransitionState state = (TransitionState)objState;

                if (state.AcceptingState == true)
                {
                    acceptingSet.AddElement(state);
                }
                else
                {
                    nonAcceptingSet.AddElement(state);
                }
            }

            if (nonAcceptingSet.GetCardinality() > 0)
            {
                arrGroup.Add(nonAcceptingSet);
            }

            arrGroup.Add(acceptingSet);

            IEnumerator iterInput = setInputSymbol.GetEnumerator();

            iterInput.Reset();

            while (iterInput.MoveNext())
            {
                string sInputSymbol = iterInput.Current.ToString();

                int nPartionIndex = 0;
                while (nPartionIndex < arrGroup.Count)
                {
                    Hashset setToBePartitioned = (Hashset)arrGroup[nPartionIndex];
                    nPartionIndex++;

                    if (setToBePartitioned.IsEmpty() || setToBePartitioned.GetCardinality() == 1)
                    {
                        continue;
                    }

                    foreach (object objState in setToBePartitioned)
                    {
                        TransitionState state = (TransitionState)objState;
                        TransitionState[] arrState = state.GetTransitions(sInputSymbol.ToString());

                        if (arrState != null)
                        {
                            Debug.Assert(arrState.Length == 1);

                            TransitionState stateTransionTo = arrState[0];

                            Hashset setFound = FindGroup(arrGroup, stateTransionTo);
                            hashMap.Add(setFound, state);
                        }
                        else
                        {
                            hashMap.Add(emptySet, state);
                        }
                    }

                    if (hashMap.Count > 1)
                    {
                        arrGroup.Remove(setToBePartitioned);
                        foreach (DictionaryEntry de in hashMap)
                        {
                            Hashset setValue = (Hashset)de.Value;
                            arrGroup.Add(setValue);
                        }
                        nPartionIndex = 0;
                        iterInput.Reset();
                    }
                    hashMap.Clear();
                }
            }

            return arrGroup;
        }

        private Hashset FindGroup(ArrayList arrGroup, TransitionState state)
        {
            foreach (object objSet in arrGroup)
            {
                Hashset set = objSet as Hashset;

                if (set.ElementExist(state) == true)
                {
                    return set;
                }
            }

            return null;
        }

        private string SetToString(Hashset set)
        {
            string s = "";
            foreach (object objState in set)
            {
                TransitionState state = (TransitionState)objState;
                s += state.Id.ToString() + ", ";
            }

            s = s.TrimEnd(new char[] { ' ', ',' });
            if (s.Length == 0)
            {
                s = "Empty";
            }
            return "{" + s + "}";
        }

        static internal void GetAllStateAndInput(TransitionState stateStart, Hashset setAllState, Hashset setInputSymbols)
        {
            Hashset setUnprocessed = new Hashset();

            setUnprocessed.AddElement(stateStart);

            while (setUnprocessed.Count > 0)
            {
                TransitionState state = (TransitionState)setUnprocessed[0];

                setAllState.AddElement(state);
                setUnprocessed.RemoveElement(state);

                foreach (object objToken in state.GetAllKeys())
                {
                    string sSymbol = (string)objToken;
                    setInputSymbols.AddElement(sSymbol);

                    TransitionState[] arrTrans = state.GetTransitions(sSymbol);

                    if (arrTrans != null)
                    {
                        foreach (TransitionState stateEpsilon in arrTrans)
                        {
                            if (!setAllState.ElementExist(stateEpsilon))
                            {
                                setUnprocessed.AddElement(stateEpsilon);
                            }
                        }
                    }
                }
            }
        }

        static internal int GetSerializedFsa(TransitionState stateStart, StringBuilder sb)
        {
            Hashset setAllState = new Hashset();
            Hashset setAllInput = new Hashset();
            GetAllStateAndInput(stateStart, setAllState, setAllInput);
            return GetSerializedFsa(stateStart, setAllState, setAllInput, sb);
        }

        static internal int GetSerializedFsa(TransitionState stateStart, Hashset setAllState,
            Hashset setAllSymbols, StringBuilder sb)
        {
            int lineLength = 0;
            int minWidth = 6;
            string line = String.Empty;
            string format = String.Empty;
            setAllSymbols.RemoveElement(MetaSymbol.EPSILON);
            setAllSymbols.AddElement(MetaSymbol.EPSILON);

            object[] symbolObjects = new object[setAllSymbols.Count + 1];
            symbolObjects[0] = "State";
            format = "{0,-8}";
            for (int i = 0; i < setAllSymbols.Count; i++)
            {
                string sSymbol = setAllSymbols[i].ToString();
                symbolObjects[i + 1] = sSymbol;

                format += " | ";
                format += "{" + (i + 1).ToString() + ",-" + Math.Max(Math.Max(sSymbol.Length, minWidth), sSymbol.ToString().Length) + "}";
            }

            line = String.Format(format, symbolObjects);
            lineLength = Math.Max(lineLength, line.Length);
            sb.AppendLine(("").PadRight(lineLength, '-'));
            sb.AppendLine(line);
            sb.AppendLine(("").PadRight(lineLength, '-'));

            int nTransCount = 0;
            foreach (object objState in setAllState)
            {
                TransitionState state = (TransitionState)objState;
                symbolObjects[0] = (state.Equals(stateStart) ? ">" + state.ToString() : state.ToString());

                for (int i = 0; i < setAllSymbols.Count; i++)
                {
                    string sSymbol = setAllSymbols[i].ToString();

                    TransitionState[] arrStateTo = state.GetTransitions(sSymbol);
                    string sTo = String.Empty;
                    if (arrStateTo != null)
                    {
                        nTransCount += arrStateTo.Length;
                        sTo = arrStateTo[0].ToString();

                        for (int j = 1; j < arrStateTo.Length; j++)
                        {
                            sTo += ", " + arrStateTo[j].ToString();
                        }
                    }
                    else
                    {
                        sTo = "--";
                    }
                    symbolObjects[i + 1] = sTo;
                }

                line = String.Format(format, symbolObjects);
                sb.AppendLine(line);
                lineLength = Math.Max(lineLength, line.Length);
            }

            format = "State Count: {0}, Input Symbol Count: {1}, Transition Count: {2}";
            line = String.Format(format, setAllState.Count, setAllSymbols.Count, nTransCount);
            lineLength = Math.Max(lineLength, line.Length);
            sb.AppendLine(("").PadRight(lineLength, '-'));
            sb.AppendLine(line);
            lineLength = Math.Max(lineLength, line.Length);
            setAllSymbols.RemoveElement(MetaSymbol.EPSILON);

            return lineLength;
        }

        public bool FindMatch(string sSearchIn, int nSearchStartAt,
                              int nSearchEndAt, ref int nFoundBeginAt, ref int nFoundEndAt)
        {
            if (_startingDFAState == null || nSearchStartAt < 0)
            {
                return false;
            }

            TransitionState stateStart = _startingDFAState;
            nFoundBeginAt = -1;
            nFoundEndAt = -1;
            bool accepted = false;
            TransitionState toState = null;
            TransitionState currentState = stateStart;
            int nIndex = nSearchStartAt;
            int nSearchUpTo = nSearchEndAt;

            while (nIndex <= nSearchUpTo)
            {
                if (_useGreedy && IsWildCard(currentState) == true)
                {
                    if (nFoundBeginAt == -1)
                    {
                        nFoundBeginAt = nIndex;
                    }
                    ProcessWildCard(currentState, sSearchIn, ref nIndex, nSearchUpTo);
                }

                char chInputSymbol = sSearchIn[nIndex];

                toState = currentState.GetSingleTransition(chInputSymbol.ToString());

                if (toState == null)
                {
                    toState = currentState.GetSingleTransition(MetaSymbol.ANY_ONE_CHAR_TRANS);
                }

                if (toState != null)
                {
                    if (nFoundBeginAt == -1)
                    {
                        nFoundBeginAt = nIndex;
                    }

                    if (toState.AcceptingState)
                    {
                        if (_matchAtEnd && nIndex != nSearchUpTo)
                        {
                        }
                        else
                        {
                            accepted = true;
                            nFoundEndAt = nIndex;
                            if (_useGreedy == false)
                            {
                                break;
                            }
                        }
                    }

                    currentState = toState;
                    nIndex++;
                }
                else
                {
                    if (!_matchAtStart && !accepted)
                    {
                        nIndex = (nFoundBeginAt != -1 ? nFoundBeginAt + 1 : nIndex + 1);

                        nFoundBeginAt = -1;
                        nFoundEndAt = -1;

                        currentState = stateStart;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (!accepted)
            {
                if (stateStart.AcceptingState == false)
                {
                    return false;
                }
                else
                {
                    nFoundBeginAt = nSearchStartAt;
                    nFoundEndAt = nFoundBeginAt - 1;
                    return true;
                }
            }

            return true;
        }

        private bool IsWildCard(TransitionState state)
        {
            return (state == state.GetSingleTransition(MetaSymbol.ANY_ONE_CHAR_TRANS));
        }

        private void ProcessWildCard(TransitionState state, string sSearchIn, ref int nCurrIndex, int nSearchUpTo)
        {
            TransitionState toState = null;
            int nIndex = nCurrIndex;

            while (nIndex <= nSearchUpTo)
            {
                char ch = sSearchIn[nIndex];

                toState = state.GetSingleTransition(ch.ToString());

                if (toState != null)
                {
                    nCurrIndex = nIndex;
                }
                nIndex++;
            }
        }

        public bool IsReady()
        {
            return (_startingDFAState != null);
        }

        public int GetLastErrorPosition()
        {
            return _lastErrorIndex;
        }

        public CompilationStatus GetLastErrorCode()
        {
            return _lastCompilationStatus;
        }

        public int GetLastErrorLength()
        {
            return _lastErrorLength;
        }

        private void UpdateValidationInfo(ValidationInfo vi)
        {
            if (vi.ErrorCode == CompilationStatus.SUCCESS)
            {
                _matchAtEnd = vi.MatchAtEnd;
                _matchAtStart = vi.MatchAtStart;
            }

            _lastCompilationStatus = vi.ErrorCode;
            _lastErrorIndex = vi.ErrorStartAt;
            _lastErrorLength = vi.ErrorLength;
        }
    }
}


