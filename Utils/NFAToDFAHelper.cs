using System.Collections;

namespace RegularExpressionParser.Utils
{

    internal class NFAToDFAHelper
    {
        private Hashtable _stateHashTable = new Hashtable();

        internal class DFAState
        {
            public Hashset SetEclosure = null;

            public bool Marked = false;
        }

        public NFAToDFAHelper()
        {

        }

        public void AddDfaState(TransitionState stateDfa, Hashset setEclosure)
        {
            DFAState stateRecord = new DFAState();
            stateRecord.SetEclosure = setEclosure;

            _stateHashTable[stateDfa] = stateRecord;
        }

        public TransitionState FindDfaStateByEclosure(Hashset setEclosure)
        {
            DFAState stateRecord;

            foreach (DictionaryEntry de in _stateHashTable)
            {
                stateRecord = de.Value as DFAState;
                if (stateRecord.SetEclosure.IsEqual(setEclosure) == true)
                {
                    return de.Key as TransitionState;
                }
            }
            return null;
        }

        public Hashset GetEclosureByDfaState(TransitionState state)
        {
            var dsr = _stateHashTable[state] as DFAState;

            if (dsr != null)
            {
                return dsr.SetEclosure;
            }
            return null;
        }

        public TransitionState GetNextUnmarkedDfaState()
        {
            DFAState stateRecord;

            foreach (DictionaryEntry de in _stateHashTable)
            {
                stateRecord = de.Value as DFAState;

                if (stateRecord.Marked == false)
                {
                    return (TransitionState)de.Key;
                }
            }

            return null;
        }

        public void MarkVisited(TransitionState stateT)
        {
            DFAState stateRecord = (DFAState)_stateHashTable[stateT];
            stateRecord.Marked = true;
        }

        public Hashset GetAllDfaState()
        {
            Hashset setState = new Hashset();

            foreach (object objKey in _stateHashTable.Keys)
            {
                setState.AddElement(objKey);
            }

            return setState;
        }
    }
}
