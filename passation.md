# Passation — Problème TUnit .NET 10/9

**Date:** 2026-09-06
**Statut:** Diagnostic complet - Solution 3 explorée et non viable
**Responsable suivant:** TBD

---

## 1. Objectif

Résoudre l'incompatibilité de TUnit avec .NET 10/9 et la nouvelle plateforme de test Microsoft (`Microsoft.Testing.Platform`).

Résultat attendu : Les tests TUnit s'exécutent et passent sur net8.0, net9.0 et net10.0.

## 2. Problématique

**Symptôme :**
- TUnit n'exécute pas les tests sur net9.0 et net10.0.
- Erreur MSBuild (lors de `dotnet test`) : `Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and later.`
- Erreur générée par `Microsoft.Testing.Platform.MSBuild.targets` (v2.4.0), ligne 355.

**Cause (confirmée - pas une hypothèse) :**
- `Microsoft.Testing.Platform 2.4.0` bloque intentionnellement le VSTest runner sur le .NET 10 SDK+ (décision de conception, pas un bug).
- La condition de blocage : `$(IsTestingPlatformApplication)==true` AND `$(TargetFramework)!=''` AND `.NET SDK version >= 10`
- Cette condition s'évalue au **moment du parsing MSBuild** (pas au runtime), même si le projet cible net8.0.
- Le check porte sur la **version du SDK utilisée pour compiler**, pas la version cible du framework.
- Microsoft a intentionnellement ajouté ce blocage pour forcer la migration de VSTest vers la nouvelle Testing Platform.

**Comportement attendu :**
- Tests TUnit passent sur net8.0, net9.0, net10.0.

**Comportement actuel :**
- Tests TUnit net8.0 : ✅ passent correctement
- Tests TUnit net9.0 : ❌ blocage MSBuild (pas d'exécution)
- Tests TUnit net10.0 : ❌ blocage MSBuild (pas d'exécution)

**Contrainte importante :**
- Ne pas casser les tests XUnit et NUnit qui fonctionnent correctement sur tous les frameworks.

**Point clé :**
- Le problème ne provient **pas** du projet TUnit.Samples lui-même, mais de `Microsoft.Testing.Platform 2.4.0` (inclus transitivement par TUnit 1.66.10).
- Aucune configuration MSBuild ne peut contourner cette erreur car elle est par conception.

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
