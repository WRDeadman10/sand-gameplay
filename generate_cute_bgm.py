import math
import struct
import wave
import numpy as np

SAMPLE_RATE = 44100
BPM = 106.0
BEAT_SEC = 60.0 / BPM
BAR_SEC = BEAT_SEC * 4.0
NUM_BARS = 16
TOTAL_DURATION = BAR_SEC * NUM_BARS
TOTAL_SAMPLES = int(SAMPLE_RATE * TOTAL_DURATION)

print(f"BPM: {BPM}, Total duration: {TOTAL_DURATION:.2f}s, Samples: {TOTAL_SAMPLES}")

# Musical pitches to frequencies
NOTE_NAMES = ['C', 'C#', 'D', 'D#', 'E', 'F', 'F#', 'G', 'G#', 'A', 'A#', 'B']
NOTE_FREQS = {}
for midi in range(12, 108):
    octave = (midi // 12) - 1
    name = NOTE_NAMES[midi % 12] + str(octave)
    NOTE_FREQS[name] = 440.0 * (2.0 ** ((midi - 69) / 12.0))


left_channel = np.zeros(TOTAL_SAMPLES, dtype=np.float64)
right_channel = np.zeros(TOTAL_SAMPLES, dtype=np.float64)

def add_wrapped_audio(audio_l, audio_r, start_sample):
    length = len(audio_l)
    for i in range(length):
        target = (start_sample + i) % TOTAL_SAMPLES
        left_channel[target] += audio_l[i]
        right_channel[target] += audio_r[i]

def synth_music_box(freq, duration_sec, pan=0.0, vel=1.0):
    """Bell-like music box: sine + pure overtones with rapid bell strike and sweet exponential decay."""
    num_samples = int(SAMPLE_RATE * (duration_sec + 0.8))
    t = np.linspace(0, num_samples / SAMPLE_RATE, num_samples, endpoint=False)
    
    # Fundamental and sweet harmonics
    wave = (
        1.00 * np.sin(2 * np.pi * freq * t) +
        0.45 * np.sin(2 * np.pi * freq * 2.002 * t) +
        0.20 * np.sin(2 * np.pi * freq * 3.004 * t) +
        0.12 * np.sin(2 * np.pi * freq * 4.008 * t) +
        0.05 * np.sin(2 * np.pi * freq * 5.012 * t)
    )
    # Gentle sparkle vibrato
    vibrato = 1.0 + 0.003 * np.sin(2 * np.pi * 5.5 * t)
    wave *= vibrato
    
    # Envelope
    env = np.exp(-t / 0.55) * (1.0 - np.exp(-t / 0.005))
    signal = wave * env * (vel * 0.28)
    
    pan_l = (1.0 - pan) * 0.5
    pan_r = (1.0 + pan) * 0.5
    return signal * pan_l, signal * pan_r

def synth_marimba(freq, duration_sec, pan=0.0, vel=1.0):
    """Warm, woody marimba / kalimba note with soft punch."""
    num_samples = int(SAMPLE_RATE * (duration_sec + 0.5))
    t = np.linspace(0, num_samples / SAMPLE_RATE, num_samples, endpoint=False)
    
    wave = (
        1.0 * np.sin(2 * np.pi * freq * t) +
        0.35 * np.sin(2 * np.pi * freq * 2.99 * t) * np.exp(-t / 0.08) +
        0.15 * np.sin(2 * np.pi * freq * 4.01 * t) * np.exp(-t / 0.04)
    )
    env = np.exp(-t / 0.35) * (1.0 - np.exp(-t / 0.003))
    signal = wave * env * (vel * 0.32)
    
    pan_l = (1.0 - pan) * 0.5
    pan_r = (1.0 + pan) * 0.5
    return signal * pan_l, signal * pan_r

def synth_plucked_ukulele(freq, duration_sec, pan=0.0, vel=1.0):
    """Soft acoustic pluck for arpeggiated cozy chords."""
    num_samples = int(SAMPLE_RATE * (duration_sec + 0.6))
    t = np.linspace(0, num_samples / SAMPLE_RATE, num_samples, endpoint=False)
    
    wave = (
        1.00 * np.sin(2 * np.pi * freq * t) +
        0.40 * np.sin(2 * np.pi * freq * 2 * t) +
        0.18 * np.sin(2 * np.pi * freq * 3 * t) +
        0.08 * np.sin(2 * np.pi * freq * 4 * t)
    )
    env = np.exp(-t / 0.40) * (1.0 - np.exp(-t / 0.004))
    signal = wave * env * (vel * 0.22)
    
    pan_l = (1.0 - pan) * 0.5
    pan_r = (1.0 + pan) * 0.5
    return signal * pan_l, signal * pan_r

def synth_bass(freq, duration_sec, pan=0.0, vel=1.0):
    """Warm, round upright pizzicato bass with cozy thumb thud."""
    num_samples = int(SAMPLE_RATE * (duration_sec + 0.4))
    t = np.linspace(0, num_samples / SAMPLE_RATE, num_samples, endpoint=False)
    
    wave = (
        1.00 * np.sin(2 * np.pi * freq * t) +
        0.45 * np.sin(2 * np.pi * freq * 2.0 * t) * np.exp(-t / 0.2) +
        0.15 * np.sin(2 * np.pi * freq * 3.0 * t) * np.exp(-t / 0.1)
    )
    env = np.exp(-t / 0.45) * (1.0 - np.exp(-t / 0.008))
    signal = wave * env * (vel * 0.42)
    
    pan_l = (1.0 - pan) * 0.5
    pan_r = (1.0 + pan) * 0.5
    return signal * pan_l, signal * pan_r

def synth_woodblock(high=True, pan=0.0, vel=1.0):
    """Soft cute woodblock tap."""
    freq = 1100.0 if high else 820.0
    num_samples = int(SAMPLE_RATE * 0.12)
    t = np.linspace(0, num_samples / SAMPLE_RATE, num_samples, endpoint=False)
    
    wave = np.sin(2 * np.pi * freq * t) + 0.3 * np.sin(2 * np.pi * freq * 2.2 * t)
    env = np.exp(-t / 0.025) * (1.0 - np.exp(-t / 0.001))
    signal = wave * env * (vel * 0.16)
    
    pan_l = (1.0 - pan) * 0.5
    pan_r = (1.0 + pan) * 0.5
    return signal * pan_l, signal * pan_r

def synth_shaker(pan=0.2, vel=1.0):
    """Soft gentle shaker sound."""
    num_samples = int(SAMPLE_RATE * 0.07)
    t = np.linspace(0, num_samples / SAMPLE_RATE, num_samples, endpoint=False)
    noise = np.random.uniform(-1, 1, num_samples)
    # High-pass like filtering by simple differencing
    noise = np.diff(noise, prepend=0)
    env = (t / 0.015) * np.exp(-t / 0.02)
    signal = noise * env * (vel * 0.11)
    pan_l = (1.0 - pan) * 0.5
    pan_r = (1.0 + pan) * 0.5
    return signal * pan_l, signal * pan_r

# Chords per bar (16 bars loop)
CHORDS = [
    # Phrase 1: Cmaj7 -> Fmaj7 -> Em7 -> Am7
    [('C3', 'E3', 'G3', 'B3'), ('C2', 'G2')],
    [('F3', 'A3', 'C4', 'E4'), ('F2', 'C3')],
    [('E3', 'G3', 'B3', 'D4'), ('E2', 'B2')],
    [('A3', 'C4', 'E4', 'G4'), ('A2', 'E3')],
    # Phrase 2: Dm7 -> G7 -> Cmaj7 -> C7
    [('D3', 'F3', 'A3', 'C4'), ('D2', 'A2')],
    [('G3', 'B3', 'D4', 'F4'), ('G2', 'D3')],
    [('C3', 'E3', 'G3', 'B3'), ('C2', 'G2')],
    [('C3', 'E3', 'G3', 'A#3'), ('C2', 'E2')],
    # Phrase 3: Fmaj7 -> Em7 -> A7 -> Dm7
    [('F3', 'A3', 'C4', 'E4'), ('F2', 'C3')],
    [('E3', 'G3', 'B3', 'D4'), ('E2', 'B2')],
    [('A3', 'C#4', 'E4', 'G4'), ('A2', 'E3')],
    [('D3', 'F3', 'A3', 'C4'), ('D2', 'A2')],
    # Phrase 4: Dm7/G (Gsus4) -> G7 -> Cmaj7 -> G7 (turnaround)
    [('F3', 'A3', 'C4', 'D4'), ('G2', 'D3')],
    [('G3', 'B3', 'D4', 'F4'), ('G2', 'F2')],
    [('C3', 'E3', 'G3', 'B3'), ('C2', 'G2')],
    [('G3', 'B3', 'D4', 'F4'), ('G2', 'D3')]
]

# 1. Add Chords & Bass & Percussion for all 16 bars
for bar_idx in range(NUM_BARS):
    bar_start_sec = bar_idx * BAR_SEC
    chord_notes, bass_notes = CHORDS[bar_idx]
    
    # Bass: Root on beat 1, Fifth on beat 3
    t1 = bar_start_sec
    t3 = bar_start_sec + 2.0 * BEAT_SEC
    b1_l, b1_r = synth_bass(NOTE_FREQS[bass_notes[0]], BEAT_SEC * 1.5, pan=0.0, vel=0.95)
    b2_l, b2_r = synth_bass(NOTE_FREQS[bass_notes[1]], BEAT_SEC * 1.5, pan=0.0, vel=0.85)
    add_wrapped_audio(b1_l, b1_r, int(t1 * SAMPLE_RATE))
    add_wrapped_audio(b2_l, b2_r, int(t3 * SAMPLE_RATE))
    
    # Plucked Ukulele Arpeggio: 8 eighth notes per bar
    # pattern: 0, 1, 2, 3, 2, 1, 2, 3
    pattern_idx = [0, 1, 2, 3, 2, 1, 2, 3]
    for step in range(8):
        step_time = bar_start_sec + step * (BEAT_SEC * 0.5)
        note_name = chord_notes[pattern_idx[step] % len(chord_notes)]
        pan = -0.35 if step % 2 == 0 else -0.25
        vel = 0.85 if step % 2 == 0 else 0.65
        pl_l, pl_r = synth_plucked_ukulele(NOTE_FREQS[note_name], BEAT_SEC * 0.9, pan=pan, vel=vel)
        add_wrapped_audio(pl_l, pl_r, int(step_time * SAMPLE_RATE))
        
    # Percussion:
    # Woodblock on beat 2 (low) and 4 (high)
    wb1_l, wb1_r = synth_woodblock(high=False, pan=0.15, vel=0.7)
    wb2_l, wb2_r = synth_woodblock(high=True, pan=0.25, vel=0.85)
    add_wrapped_audio(wb1_l, wb1_r, int((bar_start_sec + 1.0 * BEAT_SEC) * SAMPLE_RATE))
    add_wrapped_audio(wb2_l, wb2_r, int((bar_start_sec + 3.0 * BEAT_SEC) * SAMPLE_RATE))
    
    # Shaker on every 16th note (16 per bar) with accents
    for s16 in range(16):
        sh_time = bar_start_sec + s16 * (BEAT_SEC * 0.25)
        # Accent on 8th notes, ghost notes on off-16ths
        accent = 0.8 if (s16 % 4 == 2) else (0.5 if (s16 % 2 == 0) else 0.3)
        sh_l, sh_r = synth_shaker(pan=0.3, vel=accent)
        add_wrapped_audio(sh_l, sh_r, int(sh_time * SAMPLE_RATE))

# 2. Main Cute Melody (Music Box / Glockenspiel + Marimba harmony)
# Specified as (bar, beat, note, duration_beats, vel)
MELODY = [
    # Phrase 1: Bars 0-3 (Happy, welcoming, playful skip)
    (0, 0.0, 'E5', 0.75, 1.0),
    (0, 1.0, 'G5', 0.75, 0.9),
    (0, 2.0, 'C6', 1.5, 1.1),
    (0, 3.5, 'B5', 0.5, 0.8),
    
    (1, 0.0, 'A5', 1.0, 1.0),
    (1, 1.0, 'C6', 0.75, 0.85),
    (1, 2.0, 'F5', 1.5, 0.9),
    (1, 3.5, 'G5', 0.5, 0.75),
    
    (2, 0.0, 'G5', 1.0, 0.95),
    (2, 1.0, 'B5', 0.75, 0.85),
    (2, 2.0, 'E5', 1.0, 0.9),
    (2, 3.0, 'D5', 0.5, 0.75),
    (2, 3.5, 'E5', 0.5, 0.8),
    
    (3, 0.0, 'C5', 2.0, 1.0),
    (3, 2.5, 'C5', 0.5, 0.7),
    (3, 3.0, 'D5', 0.5, 0.8),
    (3, 3.5, 'E5', 0.5, 0.85),
    
    # Phrase 2: Bars 4-7 (Bouncy response)
    (4, 0.0, 'F5', 1.0, 0.95),
    (4, 1.0, 'A5', 0.75, 0.9),
    (4, 2.0, 'D5', 1.5, 0.9),
    (4, 3.5, 'E5', 0.5, 0.75),
    
    (5, 0.0, 'F5', 1.0, 0.95),
    (5, 1.0, 'G5', 0.75, 0.9),
    (5, 2.0, 'B5', 1.5, 1.0),
    (5, 3.5, 'D6', 0.5, 0.85),
    
    (6, 0.0, 'C6', 2.0, 1.1),
    (6, 2.0, 'G5', 1.0, 0.85),
    (6, 3.0, 'E5', 1.0, 0.8),
    
    (7, 0.0, 'C5', 1.5, 0.9),
    (7, 2.0, 'E5', 0.5, 0.8),
    (7, 2.5, 'G5', 0.5, 0.85),
    (7, 3.0, 'A#5', 0.5, 0.9),
    (7, 3.5, 'C6', 0.5, 0.95),
    
    # Phrase 3: Bars 8-11 (Warm, sweet peak with lovely high bells)
    (8, 0.0, 'C6', 1.5, 1.1),
    (8, 1.5, 'A5', 0.5, 0.85),
    (8, 2.0, 'F5', 1.0, 0.9),
    (8, 3.0, 'A5', 0.5, 0.85),
    (8, 3.5, 'C6', 0.5, 0.9),
    
    (9, 0.0, 'B5', 1.5, 1.0),
    (9, 1.5, 'G5', 0.5, 0.85),
    (9, 2.0, 'E5', 1.0, 0.9),
    (9, 3.0, 'G5', 0.5, 0.8),
    (9, 3.5, 'B5', 0.5, 0.85),
    
    (10, 0.0, 'A5', 1.0, 1.05),
    (10, 1.0, 'C#6', 0.75, 0.95),
    (10, 2.0, 'E6', 1.5, 1.1),
    (10, 3.5, 'D6', 0.5, 0.85),
    
    (11, 0.0, 'F6', 1.5, 1.1),
    (11, 1.5, 'D6', 0.5, 0.9),
    (11, 2.0, 'A5', 1.5, 0.95),
    
    # Phrase 4: Bars 12-15 (Playful cute run and delightful turnaround)
    (12, 0.0, 'A5', 0.5, 0.85),
    (12, 0.5, 'B5', 0.5, 0.85),
    (12, 1.0, 'C6', 1.0, 1.0),
    (12, 2.0, 'D6', 1.0, 1.0),
    (12, 3.0, 'C6', 1.0, 0.9),
    
    (13, 0.0, 'B5', 1.0, 1.0),
    (13, 1.0, 'G5', 1.0, 0.9),
    (13, 2.0, 'D5', 1.0, 0.85),
    (13, 3.0, 'F5', 1.0, 0.85),
    
    (14, 0.0, 'E5', 1.5, 1.05),
    (14, 1.5, 'G5', 0.5, 0.9),
    (14, 2.0, 'C6', 2.0, 1.15),
    
    # Bar 15 turnaround notes leading back to E5
    (15, 0.0, 'B5', 1.0, 0.9),
    (15, 1.0, 'A5', 1.0, 0.85),
    (15, 2.0, 'G5', 0.75, 0.85),
    (15, 3.0, 'F#5', 0.5, 0.8),
    (15, 3.5, 'G5', 0.5, 0.9)
]

# Harmony / Marimba counter-melody notes (answers melody with woody warmth)
COUNTER_MELODY = [
    # Bar 1 response
    (1, 2.0, 'C5', 0.5, 0.7),
    (1, 2.5, 'D5', 0.5, 0.75),
    (1, 3.0, 'E5', 0.5, 0.8),
    # Bar 3 response
    (3, 1.0, 'G4', 0.5, 0.7),
    (3, 1.5, 'A4', 0.5, 0.75),
    (3, 2.0, 'B4', 0.5, 0.8),
    # Bar 5 response
    (5, 2.0, 'D5', 0.5, 0.7),
    (5, 2.5, 'E5', 0.5, 0.75),
    (5, 3.0, 'F5', 0.5, 0.8),
    # Bar 7 little flourish
    (7, 1.0, 'C5', 0.5, 0.75),
    (7, 1.5, 'D5', 0.5, 0.8),
    # Bar 9 response
    (9, 2.0, 'G4', 0.5, 0.75),
    (9, 2.5, 'A4', 0.5, 0.8),
    # Bar 11 response
    (11, 2.0, 'D5', 0.5, 0.75),
    (11, 2.5, 'F5', 0.5, 0.8),
    # Bar 14 harmony under C6
    (14, 2.0, 'E5', 1.5, 0.8),
    (14, 2.0, 'G5', 1.5, 0.85)
]

for bar, beat, note, dur, vel in MELODY:
    t_sec = bar * BAR_SEC + beat * BEAT_SEC
    # Glockenspiel / Music box lead
    mb_l, mb_r = synth_music_box(NOTE_FREQS[note], dur * BEAT_SEC, pan=0.15, vel=vel)
    add_wrapped_audio(mb_l, mb_r, int(t_sec * SAMPLE_RATE))
    # Subtle marimba double to give it body and acoustic sweetness
    mar_l, mar_r = synth_marimba(NOTE_FREQS[note] * 0.5, dur * BEAT_SEC * 0.8, pan=0.0, vel=vel * 0.45)
    add_wrapped_audio(mar_l, mar_r, int(t_sec * SAMPLE_RATE))

for bar, beat, note, dur, vel in COUNTER_MELODY:
    t_sec = bar * BAR_SEC + beat * BEAT_SEC
    mar_l, mar_r = synth_marimba(NOTE_FREQS[note], dur * BEAT_SEC, pan=-0.2, vel=vel * 0.85)
    add_wrapped_audio(mar_l, mar_r, int(t_sec * SAMPLE_RATE))

# Normalize and master
max_val = max(np.max(np.abs(left_channel)), np.max(np.abs(right_channel)))
print(f"Max peak before norm: {max_val:.4f}")
if max_val > 0.001:
    # Target -1.5 dB peak (~0.84)
    gain = 0.84 / max_val
    left_channel *= gain
    right_channel *= gain

out_path = r"Assets\_Project\Audio\bgm_cute_loop.wav"
with wave.open(out_path, "wb") as wf:
    wf.setnchannels(2)
    wf.setsampwidth(2)
    wf.setframerate(SAMPLE_RATE)
    # Interleave 16-bit PCM
    samples_l = np.clip(left_channel * 32767.0, -32767.0, 32767.0).astype(np.int16)
    samples_r = np.clip(right_channel * 32767.0, -32767.0, 32767.0).astype(np.int16)
    interleaved = np.empty((TOTAL_SAMPLES * 2,), dtype=np.int16)
    interleaved[0::2] = samples_l
    interleaved[1::2] = samples_r
    wf.writeframes(interleaved.tobytes())

print(f"Successfully generated cute BGM to {out_path} ({TOTAL_DURATION:.1f}s, {TOTAL_SAMPLES} samples)")
