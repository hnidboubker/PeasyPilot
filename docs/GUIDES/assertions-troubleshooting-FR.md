# Guide de Dépannage des Assertions

## Problèmes Courants & Solutions

### Problème 1: "Assert/NAssert/XAssert/TAssert not found"

**Message d'Erreur:**
```
error CS0103: The name 'NAssert' does not exist in the current context
```

**Cause Racine:**
- Manque using statement
- Le projet ne référence pas le bon package PeasyPilot

**Solution:**

```csharp
// Ajouter ce using
using PeasyPilot.NUnit;  // pour NAssert
using PeasyPilot.XUnit;  // pour XAssert
using PeasyPilot.TUnit;  // pour TAssert
```

Vérifiez les références dans .csproj:
```xml
<ItemGroup>
    <ProjectReference Include="...\src\PeasyPilot.NUnit.csproj" />
</ItemGroup>
```

---

### Problème 2: IntelliSense IDE ne fonctionne pas pour les alias

**Symptôme:**
- XAssert apparaît en rouge
- Pas d'autocomplete
- Le code compile mais l'IDE montre des erreurs

**Cause Racine:**
- Fichier projet non rechargé
- Cache NuGet obsolète
- Indexation IDE non mise à jour

**Solution:**

1. **Recharger le Projet:**
   - Clic droit → Unload Project
   - Clic droit → Reload Project

2. **Nettoyer le Cache NuGet:**
   ```bash
   dotnet nuget locals all --clear
   ```

3. **Redémarrer l'IDE:**
   - Fermer et rouvrir Visual Studio

4. **Reconstruire la Solution:**
   ```bash
   dotnet clean && dotnet build
   ```

---

### Problème 3: Méthode d'Assertion de Framework Non Trouvée

**Message d'Erreur:**
```
error CS0117: 'NAssert' does not contain a definition for 'IsTrue'
```

**Cause Racine:**
- La méthode n'existe pas dans Assert de ce framework
- Typo dans le nom de la méthode
- Utilisation de la mauvaise assertion framework

**Solution:**

**Tableau de Mappage des Méthodes:**
| Objectif | NAssert | XAssert | TAssert |
|----------|---------|---------|---------|
| Vérifier True | IsTrue() | True() | await .IsTrue() |
| Vérifier Égal | AreEqual() | Equal() | await .IsEqualTo() |
| Vérifier Exception | Throws<T>() | Throws<T>() | await ThrowsAsync<T>() |

---

### Problème 4: Test Asynchrone Bloqué ou Timeout

**Symptôme:**
- Le test expire sans erreur
- Le test se bloque indéfiniment
- Seulement avec les tests asynchrones

**Cause Racine:**
- Manque `await` sur l'assertion asynchrone
- Interblocage suite à appel bloquant (`.Result`, `.Wait()`)
- Récursion asynchrone infinie

**Solution:**

```csharp
// ❌ FAUX: await manquant
[Test]
public async Task FetchUser_ReturnsUser()
{
    var user = await service.FetchUserAsync(1);
    TAssert.That(user).IsNotNull(); // Manque await - SE BLOQUE
}

// ✅ CORRECT: await TAssert
[Test]
public async Task FetchUser_ReturnsUser()
{
    var user = await service.FetchUserAsync(1);
    await TAssert.That(user).IsNotNull(); // Correct
}

// ❌ FAUX: Appel bloquant (risque d'interblocage)
[Test]
public async Task FetchUser_ReturnsUser()
{
    var user = service.FetchUserAsync(1).Result; // INTERBLOCAGE
}

// ✅ CORRECT: Utiliser await
[Test]
public async Task FetchUser_ReturnsUser()
{
    var user = await service.FetchUserAsync(1); // Correct
}
```

---

### Problème 5: Assertion d'Exception ne Capture pas l'Exception

**Symptôme:**
- Le test attend une exception mais passe
- L'exception est capturée silencieusement ailleurs
- Assert.Throws() ne se déclenche pas

**Cause Racine:**
- Exception levée sur un thread différent
- Exception capturée avant l'assertion
- Lambda n'exécute pas réellement le code

**Solution:**

```csharp
// ❌ FAUX: Exception pas dans lambda
[Test]
public void DeleteUser_ThrowsException()
{
    var user = new User();
    service.DeleteUser(user); // Exception ici, pas dans assertion
    NAssert.Throws<NotFoundException>(() => { }); // Lambda vide
}

// ✅ CORRECT: Exception dans lambda
[Test]
public void DeleteUser_ThrowsException()
{
    NAssert.Throws<NotFoundException>(() =>
    {
        service.DeleteUser(null); // Exception ICI
    });
}

// ✅ CORRECT: Assertion asynchrone
[Test]
public async Task AsyncOperation_ThrowsException()
{
    await TAssert.ThrowsAsync<InvalidOperationException>(async () =>
    {
        await service.FailingOperationAsync();
    });
}
```

---

### Problème 6: Assertion de Collection Échoue Inopinément

**Symptôme:**
- `Contains()` retourne false quand l'élément semble présent
- Nombre de collection incorrect
- Assertion Empty échoue

**Cause Racine:**
- Objects n'implémentent pas Equals
- Comparaison de références au lieu de valeurs
- Collection modifiée pendant itération

**Solution:**

```csharp
// ❌ FAUX: Objets n'implémentent pas Equals
[Test]
public void UserList_ContainsUser()
{
    var users = new List<User> { new User { Id = 1, Name = "John" } };
    var searchUser = new User { Id = 1, Name = "John" };
    
    NAssert.Contains(searchUser, users); // ÉCHOUE - objets différents
}

// ✅ CORRECT: Comparer par ID
[Test]
public void UserList_ContainsUser()
{
    var users = new List<User> { new User { Id = 1, Name = "John" } };
    var exists = users.Any(u => u.Id == 1);
    
    NAssert.IsTrue(exists); // PASSE
}
```

---

### Problème 7: Test Passe Quand Il Devrait Échouer

**Symptôme:**
- Assertion ne échoue pas même si condition est fausse
- Test passe avec mauvais résultat
- Assertion jamais exécutée

**Cause Racine:**
- Assertion pas appelée
- Mauvaise variable comparée
- Exception capturée silencieusement

**Solution:**

```csharp
// ❌ FAUX: Assertion jamais atteinte
[Test]
public void CreateUser_WithValidData_ReturnsId()
{
    var result = service.CreateUser(validUser);
    if (result <= 0) return; // Sortie prématurée - assertion ignorée!
    
    NAssert.That(result, Is.GreaterThan(0));
}

// ✅ CORRECT: Pas de sorties prématurées
[Test]
public void CreateUser_WithValidData_ReturnsId()
{
    var result = service.CreateUser(validUser);
    NAssert.That(result, Is.GreaterThan(0)); // Toujours exécutée
}
```

---

## Liste de Contrôle de Prévention

- [ ] Added `using PeasyPilot.[Framework];`
- [ ] Project references correct PeasyPilot package
- [ ] Method exists in target framework's Assert
- [ ] Async tests use `await` on async assertions
- [ ] Exception assertions include throwing code in lambda
- [ ] Objects implement `Equals()` correctly
- [ ] One assertion per test
- [ ] Test name matches what it asserts

## Ressources Associées

- [Documentation NUnit](https://nunit.org)
- [Documentation xUnit](https://xunit.net)
- [Documentation TUnit](https://www.tunit.net)
