using System.Collections;

namespace RegularExpressionParser.Utils
{
    internal class TransitionState
    {
        static private int _idCounter = 0;

        private bool _acceptingState = false;
        private int _Id = 0;

        public int Id
        {
            get
            {
                return _Id;
            }
        }

        public HashMap MatchingStateMap { get; set; }
        public bool AcceptingState
        {
            get { return _acceptingState; }
            set { _acceptingState = value; }
        }

        static public void ResetCounter()
        {
            _idCounter = 0;
        }

        public TransitionState()
        {
            _Id = _idCounter++;
            MatchingStateMap = new HashMap();
        }

        public void AddTransition(string sInputSymbol, TransitionState stateTo)
        {
            MatchingStateMap.Add(sInputSymbol, stateTo);
        }

        public TransitionState[] GetTransitions(string sInputSymbol)
        {
            if (MatchingStateMap.Contains(sInputSymbol) == true)
            {
                Hashset set = MatchingStateMap[sInputSymbol] as Hashset;
                return (TransitionState[])set.ToArray(typeof(TransitionState));
            }
            return null;
        }

        public TransitionState GetSingleTransition(string sInputSymbol)
        {
            if (MatchingStateMap.Contains(sInputSymbol) == true)
            {
                Hashset set = MatchingStateMap[sInputSymbol] as Hashset;
                return (TransitionState)set[0];
            }
            return null;
        }

        public void RemoveTransition(string sInputSymbol)
        {
            MatchingStateMap.Remove(sInputSymbol);
        }

        public int ReplaceTransitionState(TransitionState stateOld, TransitionState stateNew)
        {
            int nReplacementCount = 0;
            Hashset setTrans;
            foreach (DictionaryEntry de in MatchingStateMap)
            {
                setTrans = de.Value as Hashset;
                if (setTrans.ElementExist(stateOld) == true)
                {
                    setTrans.RemoveElement(stateOld);
                    setTrans.AddElement(stateNew);
                    nReplacementCount++;
                }
            }
            return nReplacementCount;
        }

        public bool IsDeadState()
        {
            if (_acceptingState || MatchingStateMap.Count == 0)
            {
                return false;
            }

            Hashset setToState;
            foreach (DictionaryEntry de in MatchingStateMap)
            {
                setToState = de.Value as Hashset;

                TransitionState state = null;
                foreach (object objState in setToState)
                {
                    state = objState as TransitionState;
                    if (state.Equals(this) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public ICollection GetAllKeys()
        {
            return MatchingStateMap.Keys;
        }

        public override string ToString()
        {
            string s = "s" + this.Id.ToString();
            if (!this.AcceptingState) return s;

            return "{" + s + "}";
        }
    }
}
