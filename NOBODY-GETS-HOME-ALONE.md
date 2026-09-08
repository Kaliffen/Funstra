# Nobody Gets Home Alone — Demo 06

Windows v0.6.0. Run `Build/NobodyGetsHomeAlone/Funstra.exe` from this checkout. In the portable package, extract everything and run `Funstra.exe` beside its data folder and DLLs. Completed checks, reviews and delivery receipts are recorded separately in [the validation summary](Evidence/Demo06/validation-summary.json) and [the four-reviewer dossier](Docs/funstra-review-dossier-demo06.html).

## An outing with a reason to return

Rell repairs pumps at the east workshop. Vale has impounded one component needed for his main pump. Bring that same component home through a paid release, theft or a fight. Rell can join after its return, or before the operation if you help repair his auxiliary pump. Neri retains the clinic partnership: obtain the six impounded medicine doses, donate them and ask Neri to join.

1. Begin alone. Visit Rell at the east workshop and press **E** to hear the terms. To recruit him without fighting, accept the auxiliary repair, approach its pump and **hold E for 12 seconds**. The repair consumes the workshop's last gasket and drains the sump. Completed work earns mechanics practice; interrupted work does not.
2. Prepare at Sella in Market Court. Her paper-delivery favor to Neri earns a finite pistol and six rounds; other guns and ammunition cost money. Mara's work and cargo can fund supplies or Vale's release. Use the protagonist for these existing trade, clinic and cargo interactions.
3. Compare the yard approaches on separate saves or fresh nights. **Bargain:** take $90 to Vale's public quay counter while he is conscious and available; collect the cleared component inside. **Steal:** use the public quay or southern service gate, watch actual sightlines, then hold E to unfasten it. **Fight:** assemble and equip the crew, give positions and engage the same ordinary-health guards. Violence ends safe passage. There is one component, not a separate reward for each approach.
4. Carry the component to Rell at the east workshop and press E. Its carrier owns it even while another person is selected. A nearby selected crew member can press E to take custody from its current carrier. Return resolves Rell's material stake and earns his partnership if he has not joined already. Bring recruited Rell back to his workshop to complete the return.
5. Repeat the approach with only Neri, then with all three people. Compare resources, attention and recovery options, not just the number of guns. The yard watch communicates sightings; removing its leader or interrupting a report changes coordination. A rifle favors deliberate fire and longer lanes; it does not make its holder invulnerable.

## Crew controls and equipment

| Input | Action |
|---|---|
| WASD / Shift / Ctrl or C | Move / sprint / sneak |
| F1 / F3 / F4 | Control protagonist / recruited Neri / recruited Rell |
| Mouse / left mouse | Aim / fire or strike |
| 1 | Holster, use fists |
| 2 / 3 / 4 / 5 | Protagonist pistol / shotgun / SMG / rifle; a companion draws their single carried gun |
| R / B | Reload / begin timed self-aid |
| E / hold E | Nearby interaction / work or collection shown by the prompt |
| K | Pause and open crew orders, transfers and rescue |
| G / H / T | Follow / hold / aid for the partner addressed in the crew panel |
| Space / Escape | Tactical pause and resume / pause menu or close dialogue |
| Tab / J / F5 | Map / incident history / save |
| F2 / F11 / M | Compact HUD / fullscreen / mute |

In **K**, first choose the person to address. This does not change who you control. Follow follows the controlled person; Hold stays put; Move To asks for a destination click; Retreat heads for the clinic; Cover / Engage orders an armed partner to fight. Resume time after issuing orders. Select another person with F1/F3/F4 when you want direct control instead.

Stand close with clear access to transfer a dressing or **the held gun and all of its rounds**. A companion holds one gun. Its magazine and unfinished reload move with it; the donor loses custody. Transfers cannot duplicate equipment. Select the donor to give equipment back. A holstered companion still owns the gun. Visible illegal weapons, shots and witnessed attacks can bring police pressure.

## Getting someone home

Field aid takes three uninterrupted seconds, requires a nearby patient and spends one dressing only when completed. Moving away or taking a direct hit interrupts it. Successful treatment earns medicine practice and a remembered encounter.

When the protagonist goes down, a living recruited partner can continue at their own position. In K, address the downed person, choose **Carry Downed**, resume and physically travel through a clear route. Carrying slows movement. Use **Drop** for a reachable local placement. At the clinic, **Admit at Clinic / 1 Dose** consumes existing medical stock and records who brought the patient back. A clinic without stock cannot provide this admission.

Returning to shelter and choosing **Leave Them Behind** records explicit abandonment; it does not transport the casualty. Save and reload to inspect condition, position and memory. If the entire recruited crew is incapacitated, emergency recovery has its own loss/debt summary. Human judgment of the emotional weight of these outcomes remains part of review.

An operation can cost people or fail outright. You are free to withdraw and leave someone behind; a clean return is not guaranteed. This build represents zero health as incapacitation, with persistent bodies and abandonment, and retains the existing total-defeat emergency recovery. It does not yet model a separate irreversible death state. The wider world direction permits death without rescue; the current evidence does not claim that later mechanic is implemented.

## Existing streets and saves

Neri's existing clinic services, Ivo, Sella, Tomas, Mara, loose cargo and home services are protagonist interactions. Selected companions operate the new dock and crew/rescue systems. Use F1 for the older services. Tomas still carries three finite consignments; the same money, medicine and supply custody matter. Named residents respond to perceived danger and can spend finite dressings to help; observe their reasons and remembered encounters.

The campaign uses `crew-progress.json` in `%USERPROFILE%/AppData/LocalLow/Funstra/Funstra`. Continue can import the earlier arms/street saves when no crew save exists, preserving those originals. F5 and autosave preserve crew identities, selection, orders, equipment, wounds and carried goods. The title **DEBUG FAST RUN** toggle starts off, lasts only for the application session and is not stored in the campaign. Identify its use when comparing travel or pacing.

The increment has three controllable people, one contested component, Vale and two yard guards, and the existing two finite police truck deliveries with a nine-officer ceiling. The rifle extends that response. Army scale, grenades and 30-plus soldiers remain later work.

## Guided review and evidence

The [shared weighted outcome ledger](Evidence/Demo06/review-outcomes.json) defines the panel's commitments. It assigns no verdicts. Reviewers use [evaluation protocol v2](reviewer-agents/README.md): **Game now /10 · Slice delivery /10 · Evidence %**, with their fixed category weights, explicit deficits and evidence limits. The [Demo06 dossier](Docs/funstra-review-dossier-demo06.html) is produced after all four reviews; its absence means panel work is unfinished.

Use the identified existing executable with `Tools/Test-Crew.ps1 -PlayerPath Build/NobodyGetsHomeAlone/Funstra.exe -EvidenceRoot <unique-folder>`. Add `-ApproachOnly` in a separate output folder for the solo theft, Neri pair and full-crew combat route. Both unseen routes check successful return; combat records the actual return, costly withdrawal or total-defeat emergency recovery in `crew-combat-outcome.json`, then verifies saved identities, custody, wounds, positions and ammunition through reload. Raw positions are retained; only an uncarried protagonist may gain 0 through 0.1201 vertical units from existing resume clearance. Horizontal coordinates, companion positions, carried-body placement and all other recorded fields must match exactly. Combat success and a clean crew return are not required, and an unresolved operation is reported as a mission loss. `-FightOnly` and `-OppositionOnly` are narrower diagnostic replays; neither establishes all the approach outcomes. Read the current runner before executing additional flags. The core route mixes actual scripted controller journeys with explicitly prepared money, recruitment, wounds, equipment and combat fixtures. It checks selection and in-flight owner persistence, finite transfers, interrupted aid, physical carry travel, return/abandonment, total defeat, repair and opposition. In live post-retreat encounters, aid may complete or end through a verified hit interruption; pressure also permits its actual cancellation when officers regain contact. Interruption must spend no dressing and grant no healing or practice. The evidence reports the outcome rather than guaranteeing treatment under pursuit. These are muted guided exported-player tests, not free exploration or human input acceptance.

Final acceptance evidence is expected under `Evidence/Demo06/crew-final`, `approach-final`, `arms-final`, `residents-final`, `streets-final`, `police-final`, `pressure-final` and `legacy-final`. Use `Test-Arms.ps1`, `Test-Residents.ps1` and `Test-Streets.ps1` with their explicit `-PlayerPath` overrides for regressions; their defaults may name earlier demos. The integrated rifle/crew pressure route is `Test-Streets.ps1 -Mode Pressure -WithCrew`, with the same explicit executable and isolated output folder. Crew/approach coverage must identify any prepared scenes, frozen AI, direct transactions, accelerated simulation and controller travel. Read each `build-identity.json`, result, machine-readable snapshots and complete player log, then open relevant rendered frames at street, clinic, yard, pause and recovery. An assertion count or a still alone does not establish an entire journey.

Reviewer runtime outputs belong in `Evidence/Demo06/review-{dag,priya,marcus,nell}`; original reviews and meeting notes belong in the corresponding `reviewer-agents/<persona>/demo06*.md`. Run players serially: runners share a validation mutex and test-save names. Do not overwrite a completed evidence folder. Fresh matching identity and a PASS are prerequisites, not a score or proof of minimum-spec performance.

`Tools/Package-Crew.ps1 -ExpectedAssemblySha256 <tested-sha256> -ValidateOnly` checks final package prerequisites without writing an archive. Packaging uses the tested files without rebuilding and produces `Releases/Funstra-nobody-gets-home-alone-0.6.0-windows.zip`. Record source commit/dirty state separately from binary and archive hashes; an executable hash alone does not prove which checkout produced it. [The pipeline](Docs/PIPELINE.md#demo06-reviewed-package-and-publication) specifies portable checks and publication. Website verification, retained release downloads and explicit owner acceptance are separate recorded gates.
