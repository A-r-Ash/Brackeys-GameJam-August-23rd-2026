# Brackeys Game Jam — August 23rd 2026

7-day team jam. Read this before you touch the project.

---

## 1. Unity version — use this EXACT one

```
6000.4.6f1
```

Install it through **Unity Hub**. Not "Unity 6", not "close enough" — the exact string above.
A one-digit mismatch corrupts scenes and prefabs for whoever is off. Confirm your version before you open the project.

---

## 2. Folder rules — where things go

Everything lives under `Assets/`:

| Folder             | What goes here                                  | Who touches it        |
| ------------------ | ----------------------------------------------- | --------------------- |
| `Assets/Scripts`   | All C# code                                      | Programmers only      |
| `Assets/Art`       | Sprites, textures, `.psd`, exported art          | Artists drop files here |
| `Assets/Audio`     | Music + SFX (`.wav`, `.mp3`, `.ogg`)             | Composer drops files here |
| `Assets/Prefabs`   | Prefabs (characters, enemies, UI pieces, etc.)   | Everyone builds here  |
| `Assets/Scenes`    | Unity scenes                                     | **Scene owner only**  |

**Artists:** you do not need to open Unity. Make art in your own tools, export into `Assets/Art`, commit. A programmer wires it into the game.

---

## 3. THE RULE THAT SAVES THE JAM — one scene owner

**Only one person edits the main scene.** (That's the tech lead — Arash.)

Git cannot merge two people's edits to the same Unity scene file. Two people in the scene at once = corrupted scene = lost work. This is how team jams die.

Everyone else works inside **prefabs**, never directly in the main scene. The scene owner drags prefabs in. Modern nested prefabs make this painless.

---

## 4. Git workflow — every single time

Whether you use GitHub Desktop (recommended for artists) or the command line, the loop is the same:

1. **Pull first** — always sync before you start working. Never skip this.
2. **Work** — make your change (art, audio, a prefab, a script).
3. **Commit** — small and often, with a clear message ("add player sprite", not "stuff").
4. **Push** — send it up so the team has it.

**GitHub Desktop:** Fetch/Pull → make changes → write a summary → Commit to main → Push origin.

**Command line:**
```bash
git pull
# ...do your work...
git add -A
git commit -m "clear message here"
git push
```

### Ground rules
- **Pull before you work.** Most conflicts come from skipping this.
- **Commit small, push often.** Don't sit on 6 hours of uncommitted work.
- **Never leave the repo broken overnight.** If it doesn't compile, it blocks everyone. Don't push broken code and log off.
- **Talk on voice constantly.** "I'm editing the player prefab" out loud prevents two people colliding.

---

## 5. Binaries are handled (Git LFS)

The repo is already set up with **Git LFS** for images, audio, and 3D files (`.png`, `.psd`, `.wav`, `.fbx`, etc.). GitHub Desktop includes LFS automatically — you don't need to do anything. Just drop your files in the right folder and commit.

---

## 6. Builds & submission

- **Target = WebGL**, uploaded to itch.io (that's how Brackeys judges play it).
- **Only the tech lead makes builds.**
- First WebGL build goes up **Day 1** (gray boxes are fine) — WebGL breaks things that work in the editor, and you want to find that out early, not on Day 6.
- New build every ~12 hours after that, uploaded as an unlisted/draft on itch.io. **Always be submittable.**

---

## 7. Team

| Role        | Person   |
| ----------- | -------- |
| Tech lead / programmer / scene owner / builds | Arash |
| Programmer / artist | — |
| Artist | — |
| Artist | — |
| Composer | — |

---

**One finished small game beats an unfinished ambitious one. "Shipped" is the only word that matters.**
