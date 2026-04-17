# 🍣 VR Japanese Restaurant

> Projet Unity VR — IIM Digital School  
> Simulation immersive d'un restaurant de style japonais avec interactions en réalité virtuelle.

---

## ⚠️ Note importante

> **Le setup VR n'était pas fonctionnel** lors du développement du projet. Les interactions et la navigation sont partiellement opérationnelles, mais le pipeline VR complet reste instable.

---

## 🏯 Environnement

Restaurant de style japonais composé d'un **extérieur modélisé** (façade, devanture) et d'un **intérieur complet** (salle, cuisine, décoration).

---

## ✅ Contenu de la scène

### 🏠 Salle
- Sol, murs et plafond modélisés

### 💡 Lumières
- Interrupteur pour allumer / éteindre les lumières

### 🚪 Porte d'entrée
- Cliquer sur la poignée ouvre ou ferme la porte

### ✨ Spawn d'objets
Input dédié pour faire apparaître des objets :
- **Table**
- **Chaise**
- **Poisson**

### 🔄 Reset
- Input qui remet la scène dans son état initial

### 🖥️ Interface de sélection
- Input qui ouvre une interface permettant de choisir l'objet à faire apparaître

### 🎮 Manipulation d'objets
En ciblant un objet et en maintenant l'input enfoncé :
- **Déplacement** — drag & drop de l'objet dans la scène
- **Rotation** — faire pivoter l'objet sur lui-même
- **Suppression** — supprimer l'objet de la scène

### 🐼 Interactions bonus
- Interaction avec le **Panda**
- Interaction avec l'**Octopus**
- Interaction avec le **four**
- Interaction avec le **frigo**

---

## 👥 Répartition des tâches

### Thomas
- Setup VR
- Spawn d'objets
- Déplacement, rotation et suppression d'objets
- Initiation du projet
- Init HandController

### Yohan
- Modélisation de l'extérieur
- Quelques animations
- Script de déplacement

### Ilyan
- Modélisation intérieure complète
- Interactions
- VR
- Scripts

---

## 🛠️ Stack technique

- **Moteur** : Unity
- **VR** : OpenXR
- **Plateforme cible** : Android (APK)


*IIM Digital School — Projet VR Unity*
