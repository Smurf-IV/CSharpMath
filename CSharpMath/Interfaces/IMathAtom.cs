﻿using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;
using CSharpMath.Atoms;

namespace CSharpMath.Interfaces {
  public interface IMathAtom {
    string StringValue { get; }
    MathItemType ItemType { get; set; }
    string Nucleus { get; set; }
    IMathList Superscript { get; set; }
    IMathList Subscript { get; set; }
    FontStyle FontStyle { get; set;}
    Range IndexRange { get; }

    /// <summary>
    /// Whether or not the atom allows superscripts and subscripts.
    /// </summary>
    bool ScriptsAllowed();

    /// <summary>
    /// If this atom was formed by fusion of multiple atoms, then this stores the list
    /// of atoms that were fused to create this one. This is used in the finalizing
    /// and preprocessing steps.
    /// </summary>
    List<IMathAtom> FusedAtoms { get; }

    T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper);
  }
}