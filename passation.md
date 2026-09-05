# Passation — Problème TUnit .NET 10/9

**Date:** 2026-09-05
**Statut:** En cours
**Responsable suivant:** TBD

---

## 1. Objectif

Résoudre l'incompatibilité de TUnit avec .NET 10/9 et la nouvelle plateforme de test Microsoft (`Microsoft.Testing.Platform`).

Résultat attendu : Les tests TUnit s'exécutent et passent sur net8.0, net9.0 et net10.0.

## 2. Problématique

**Symptôme :**
- TUnit n'exécute pas les tests sur net9.0 et net10.0.
- Erreur de build : `Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and later.`
- Le workflow CI peut continuer car le filtre test exclut ces cibles, mais c'est un contournement temporaire.

**Cause (hypothèse) :**
- TUnit repose sur l'ancienne cible VSTest, dépréciée sur .NET 10+.
- Incompatibilité profonde entre TUnit et Microsoft.Testing.Platform 2.4.0.

**Comportement attendu :**
- Tests TUnit passent sur net8.0, net9.0, net10.0.

**Comportement actuel :**
- Tests ne s'exécutent pas du tout sur net9.0/net10.0 (102 tests, aucun message d'erreur clair).

**Contrainte importante :**
- Ne pas casser les tests XUnit et NUnit qui fonctionnent correctement.

## 3. Fichiers importants

| Fichier | Rôle | Importance |
|---------|------|-----------|
| `samples/PeasyPilot.TUnit.Samples/PeasyPilot.TUnit.Samples.csproj` | Configuration projet TUnit | Contient TargetFrameworks et config de plateforme de test |
| `.github/workflows/build.yml` | Workflow CI | Filtre test avec `Category!=net10-skip&Trait!=net10-skip` |
| Issue #13 GitHub | Tracking du problème | Documentation des tentatives échouées et résultats |

## 4. Ce qui a été essayé et n'a pas fonctionné

**Tentative 1 — Ajouter `<UseNewTestingPlatform>true</UseNewTestingPlatform>` au .csproj**

Approche :
- Modifier le PropertyGroup de TUnit.Samples.csproj pour opt-in à la nouvelle plateforme Microsoft.

Résultat :
- Build réussit, mais **102 tests échouent silencieusement** (aucun message d'erreur explicite).

Pourquoi ça n'a pas fonctionné :
- La propriété seule est insuffisante. TUnit nécessite une refonte plus profonde pour être compatible avec la nouvelle plateforme.

Conclusion :
- Cette approche ne résout pas le problème.

---

**Tentative 2 — Mettre à jour TUnit + ajouter la propriété**

Approche :
- Mettre à jour le package TUnit à sa dernière version.
- Ajouter `<UseNewTestingPlatform>true</UseNewTestingPlatform>`.

Résultat :
- **102 erreurs persistent** (toujours sans message clair).

Pourquoi ça n'a pas fonctionné :
- La dernière version de TUnit n'est pas compatible avec Microsoft.Testing.Platform sur net9.0/net10.0.

Conclusion :
- TUnit a un bug de compatibilité réel, indépendant de la version du package.

## 5. Solutions envisagées

**Solution 1 — Limiter TUnit à net8.0 uniquement (recommandée)**

Description :
- Changer `TargetFrameworks` de `net8.0;net9.0;net10.0` à `net8.0`.
- Les tests TUnit continueraient de fonctionner sur net8.0.

Pourquoi ça pourrait fonctionner :
- TUnit fonctionne correctement sur net8.0 (testé et validé).
- Élimine l'erreur de build immédiatement.

État actuel :
- Non testée (mais logiquement viable).

Risques / inconvénients :
- Perte de couverture de test TUnit sur net9.0/net10.0.
- Les samples TUnit ne validaraient pas les versions récentes.

Ce qui reste à faire :
- Appliquer le changement.
- Vérifier que les tests TUnit net8.0 passent.
- Documenter cette limitation dans PROJECT_MEMORY.md.

---

**Solution 2 — Attendre une fix officielle de TUnit**

Description :
- Aucune action. Surveiller les releases TUnit pour une version compatible.

État actuel :
- Aucun ETA connu pour une fix officielle.

Risques / inconvénients :
- Délai indéterminé.
- Le workflow CI continue d'afficher l'avertissement.

---

**Solution 3 — Explorer une configuration MSBuild alternative**

Description :
- Chercher si une autre propriété ou configuration existe pour forcer la compatibilité.
- Exemple : `<UseVSTestRunner>false</UseVSTestRunner>` ou équivalent.

État actuel :
- À explorer ; aucune configuration documentée connue pour TUnit.

Risques / inconvénients :
- Peut ne pas exister ou ne pas résoudre le problème.

Ce qui reste à faire :
- Consulter la documentation officielle TUnit sur Microsoft.Testing.Platform.

## 6. État actuel du travail

- **Issue #13 créée** sur GitHub : "[AUTO] TUnit Microsoft.Testing.Platform compatibility - .NET 9/10"
  - URL: https://github.com/hnidboubker/PeasyPilot/issues/13
- **Branche créée** : `fix/tunit-net10-platform` (aucune modification finalisée)
- **Code** : Revert à l'état original (aucune solution appliquée)
- **Conclusion dans Issue #13** : Non-bloquant mais sans solution simple immédiate.

## 7. Prochaines étapes recommandées

1. **Immédiat** : Appliquer Solution 1 (limiter TUnit à net8.0) pour dégager le problème.
2. **Documentation** : Mettre à jour PROJECT_MEMORY.md avec cette limitation.
3. **Surveillance** : Suivre les releases de TUnit pour une version compatible.
4. **Alternative** : Explorer Solution 3 si le temps le permet.

---

**Dernière mise à jour:** 2026-09-05 21:15 UTC
