﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SATInterface
{
    public class BoolVar:BoolExpr
    {
        internal readonly int Id;
        internal bool Value;
        internal readonly Model Model;
        private readonly string Name;
        private BoolExpr negated;

        internal BoolExpr Negated
        {
            get
            {
                if (ReferenceEquals(negated, null))
                    negated = new NotExpr(this);
                return negated;
            }
        }

        public BoolVar(Model _model):this(_model,"b"+(_model.VarCount+1))
        {
        }

        public BoolVar(Model _model,string _name)
        {
            Model = _model;
            Name = _name;
            Id = ++_model.VarCount;
            Model.RegisterVariable(this);
        }

        internal BoolVar(string _name)
        {
            Name = _name;
        }

        public override string ToString() => Name;

        public override bool X
        {
            get
            {
                if (ReferenceEquals(this, TRUE))
                    return true;
                if (ReferenceEquals(this, FALSE))
                    return false;

                return Value;
            }
        }

        internal override IEnumerable<BoolVar> EnumVars()
        {
            yield return this;
        }

        public override int GetHashCode() => Id;
    }
}
