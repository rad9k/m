## Research

### Mobile Phone Localization Patents

I am a co-inventor of two patents focused on **practical mobile phone localization in GSM networks**. Together, they address a specific limitation of conventional cellular positioning: standard methods work reasonably well when the network is trying to locate a **known subscriber or device**, but they are much less useful when the goal is to detect **which phones are present in a specific physical place**. :contentReference[oaicite:0]{index=0} :contentReference[oaicite:1]{index=1}

---

### The Problem

Classical GSM localization typically estimates a phone’s position by analyzing signal timing or signal strength relative to multiple base stations, and then applying a form of geometric triangulation. This can provide only coarse accuracy, usually on the order of hundreds of meters, and it assumes the network is trying to localize a **particular device** that is already identified. :contentReference[oaicite:2]{index=2}

That model is useful when you know **which phone** you are looking for. But it does not solve a different and operationally important problem:

> **How do you detect that an arbitrary mobile phone has entered a specific location, even if you do not know its number in advance?**

This is the harder problem in location-aware services, presence detection, venue analytics, or access-sensitive environments. Standard network-based localization is too coarse, and traditional approaches do not naturally support continuous detection of **any phone appearing in a small, predefined area**. The Polish patent explicitly identifies this limitation: known methods require a known number or identifier and do not provide immediate detection of any handset present in a concrete geographic zone. :contentReference[oaicite:3]{index=3}

---

### Patent 1: Localization by Low-Power Mini BTS

The first patent proposes a simple but powerful idea: instead of trying to infer position only from large, city-scale BTS cells, place a **low-power mini BTS** at the location of interest. When a phone moves close enough to that mini BTS, the received signal from it becomes stronger than the signal from the surrounding high-power urban BTS stations. As a result, the phone attaches through the mini BTS, and the network can treat the phone’s location as the location of that mini BTS. :contentReference[oaicite:4]{index=4}

In other words, the method turns localization from a broad estimation problem into a **controlled detection event**:

1. A mini BTS is installed in a place where phone presence should be monitored.
2. A nearby phone enters its small coverage zone.
3. The phone prefers the mini BTS because its local signal becomes dominant.
4. The GSM network records that event.
5. The system reads the mini BTS position from a database and assigns that position to the phone. :contentReference[oaicite:5]{index=5}

This improves localization accuracy from the typical **300–500 m** range of standard GSM methods to roughly **20–50 m**, which is precise enough to detect presence in a narrowly defined area rather than just somewhere inside a large urban cell. :contentReference[oaicite:6]{index=6}

#### Why this matters

The key innovation is not only better accuracy. It is the ability to detect **phones present in a specific place without needing to know their numbers beforehand**. The place itself becomes the trigger. Instead of asking, “Where is this known phone?”, the system can answer, “Which phones have entered this monitored micro-area?” That is the conceptual shift introduced by the first patent. :contentReference[oaicite:7]{index=7}

---

### Patent 2: Faster and More Reliable Detection Using the SIM Card

The second patent extends the original concept and makes it more robust. It observes that relying on a full network log-in or location update via the mini BTS can be too slow in practice. A moving phone may pass through the mini BTS coverage area before the standard GSM connection procedure fully completes. In that case, the localization opportunity can be missed. The European patent identifies this as a core weakness of the earlier approach. :contentReference[oaicite:8]{index=8}

To solve this, the second patent introduces a more proactive mechanism based on software operating on the **SIM card**. Instead of waiting only for a completed network attachment, the SIM-resident program monitors changes in the list of strongest received base-station signals. When a new base station appears on that list, the software checks whether its identifier belongs to the range assigned to localization mini BTS stations. If yes, it triggers a localization event and informs the exchange, which then reads the mini BTS position from the database and assigns it to the phone. :contentReference[oaicite:9]{index=9}

This design improves the system in two important ways:

- it makes detection **faster**, because the event can be triggered as soon as the relevant mini BTS becomes visible in the phone’s radio environment, rather than only after full connection procedures finish;
- it makes detection **more reliable**, because localization can be triggered even in cases where the mini BTS signal is merely comparable to surrounding signals, provided it meets the defined criteria in the SIM-side logic. :contentReference[oaicite:10]{index=10}

The European patent also states localization accuracy in the **20–50 m** range, preserving the fine-grained spatial targeting of the first invention while improving responsiveness. :contentReference[oaicite:11]{index=11}

---

### Technical Contribution

Taken together, the two patents define a practical architecture for **micro-location detection in cellular networks**:

- use **low-power dedicated base stations** to create small, well-defined detection zones;
- map those stations to physical coordinates in a database;
- interpret the phone’s radio behavior as evidence of presence in that zone;
- in the improved version, use **SIM-level logic** to detect the event earlier and more reliably. :contentReference[oaicite:12]{index=12} :contentReference[oaicite:13]{index=13}

This approach is technically interesting because it does not depend on GPS, does not require the handset to disclose its location explicitly, and does not begin with knowledge of a particular phone number. Instead, it treats localization as an **infrastructure-driven event tied to a place**, which is exactly what many real-world services need. :contentReference[oaicite:14]{index=14} :contentReference[oaicite:15]{index=15}

---

### Patent List

- **PL 197247 B1 — “Sposób lokalizacji telefonu komórkowego”**  
  Core concept: localization by introducing the phone into the coverage area of a low-power mini BTS and assigning the phone the geographic position of that station. :contentReference[oaicite:16]{index=16}

- **EP 2 003 466 B1 — “Mobile phone localization method”**  
  Extended concept: faster and more reliable localization using SIM-card software that detects the appearance of a localization mini BTS in the phone’s strongest-signal list. :contentReference[oaicite:17]{index=17}