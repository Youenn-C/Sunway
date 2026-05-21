using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// =============================================================================
/// INTERFACE : IRailConnectable
/// =============================================================================
/// 
/// CONCEPT CLÉ : Le "Langage Commun" du Réseau
/// ---------------------------------------------------------
/// Imaginez que votre réseau ferré est une fête où se trouvent des invités très différents :
/// - Des Gares (gros bâtiments statiques)
/// - Des Tronçons de Rail (vos splines courbes)
/// - Futurs Aiguillages (mécanismes complexes)
///
/// PROBLÈME : Le Train ne doit pas avoir un code différent pour parler à une Gare 
/// ("if objet is Station") et un autre pour parler à un Rail.
///
/// SOLUTION : Cette Interface agit comme un "Badge" que tous ces objets portent.
/// Peu importe leur nature réelle, s'ils ont ce badge, ils PROMETTENT de savoir 
/// répondre à deux questions précises. C'est ce qu'on appelle le POLYMORPHISME.
///
/// AVANTAGE : Vous pourrez ajouter n'importe quel nouvel objet (Pont, Tunnel, Port)
/// plus tard sans jamais modifier le code du Train. Il suffira que le nouvel objet
/// implémente cette interface.
/// =============================================================================
/// </summary>
public interface IRailConnectable
{
    /// <summary>
    /// QUESTION 1 : "Quels sont les rails qui partent de chez toi ?"
    /// 
    /// - Une GARE répondra : "J'en ai 3 (Nord, Sud, Est)".
    /// - Un RAIL répondra : "J'en a 1 (celui qui est attaché à mon extrémité)".
    /// - Un AIGUILLAGE répondra : "Cela dépend de ma position actuelle".
    ///
    /// Le Train utilise cette liste pour savoir quelles sont ses options de trajet.
    /// </summary>
    /// <returns>Liste des tronçons de rail accessibles depuis cet objet.</returns>
    List<SplineRail> GetConnectedRails();

    /// <summary>
    /// QUESTION 2 : "Où es-tu situé exactement dans le monde ?"
    /// 
    /// Utile pour :
    /// - Que le train sache où il arrive (pour s'arrêter précisément).
    /// - Le système de construction pour détecter les connexions automatiques (Snap).
    /// </summary>
    /// <returns>La position mondiale (Vector3) de l'objet.</returns>
    Vector3 GetPosition();
}