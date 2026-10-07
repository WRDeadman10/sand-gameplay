import math
import struct
import wave
import numpy as np

SAMPLE_RATE = 44100

def save_wav(filename, left_channel, right_channel=None):
    if right_channel is None:
        right_channel = left_channel
    
    max_val = max(np.max(np.abs(left_channel)), np.max(np.abs(right_channel)))
    if max_val > 0.001:
        gain = 0.86 / max_val
        left_channel = left_channel * gain
        right_channel = right_channel * gain
        
    num_samples = len(left_channel)
    samples_l = np.clip(left_channel * 32767.0, -32767.0, 32767.0).astype(np.int16)
    samples_r = np.clip(right_channel * 32767.0, -32767.0, 32767.0).astype(np.int16)
    interleaved = np.empty((num_samples * 2,), dtype=np.int16)
    interleaved[0::2] = samples_l
    interleaved[1::2] = samples_r
    
    with wave.open(filename, "wb") as wf:
        wf.setnchannels(2)
        wf.setsampwidth(2)
        wf.setframerate(SAMPLE_RATE)
        wf.writeframes(interleaved.tobytes())
    print(f"Saved: {filename} ({num_samples / SAMPLE_RATE:.2f}s)")

# ============================================================
# 1. SFX: PICK BOX (Cute upward pop / lift)
# ============================================================
def generate_pick_box():
    duration = 0.14
    num_samples = int(SAMPLE_RATE * duration)
    t = np.linspace(0, duration, num_samples, endpoint=False)
    
    # Rapid upward pitch sweep 280Hz -> 620Hz
    freq = 280.0 + 340.0 * (t / duration) ** 0.8
    phase = 2 * np.pi * np.cumsum(freq) / SAMPLE_RATE
    
    # Fundamental + warm 2nd harmonic
    sig = np.sin(phase) + 0.35 * np.sin(2.0 * phase)
    # Bell transient attack, quick exponential decay
    env = np.sin(np.clip(t / 0.015, 0, 1) * (np.pi / 2)) * np.exp(-t / 0.045)
    wave_data = sig * env
    save_wav(r"Assets\_Project\Audio\sfx_pick_box.wav", wave_data)

# ============================================================
# 2. SFX: DROP BOX (Satisfying soft thud / placement)
# ============================================================
def generate_drop_box():
    duration = 0.16
    num_samples = int(SAMPLE_RATE * duration)
    t = np.linspace(0, duration, num_samples, endpoint=False)
    
    # Pitch drop 260Hz -> 100Hz
    freq = 100.0 + 160.0 * np.exp(-t / 0.035)
    phase = 2 * np.pi * np.cumsum(freq) / SAMPLE_RATE
    
    # Woody body + soft click click at t=0
    click = np.random.uniform(-0.5, 0.5, num_samples) * np.exp(-t / 0.006)
    sig = np.sin(phase) + 0.4 * np.sin(2.0 * phase) + 0.2 * np.sin(3.0 * phase)
    env = (1.0 - np.exp(-t / 0.005)) * np.exp(-t / 0.04)
    wave_data = sig * env + click * 0.4
    save_wav(r"Assets\_Project\Audio\sfx_drop_box.wav", wave_data)

# ============================================================
# 3. SFX: DRAG LOOP (Smooth sliding / board friction)
# ============================================================
def generate_drag_loop():
    duration = 1.0  # 1-second seamless loop
    num_samples = int(SAMPLE_RATE * duration)
    t = np.linspace(0, duration, num_samples, endpoint=False)
    
    # Low frequency wood rumble (approx 95 Hz + harmonics)
    rumble = 0.4 * np.sin(2 * np.pi * 95 * t) + 0.2 * np.sin(2 * np.pi * 190 * t)
    
    # Filtered friction noise (white noise difference for soft high shelf)
    noise = np.random.uniform(-1, 1, num_samples)
    smooth_noise = np.convolve(noise, np.ones(12)/12, mode='same')
    
    # Subtle amplitude modulation for sliding texture
    am = 0.85 + 0.15 * np.sin(2 * np.pi * 7.5 * t)
    wave_data = (smooth_noise * 0.5 + rumble * 0.5) * am
    
    # Ensure seamless loop crossfade at ends
    fade_len = int(SAMPLE_RATE * 0.05)
    fade_in = np.linspace(0, 1, fade_len)
    wave_data[:fade_len] = wave_data[:fade_len] * fade_in + wave_data[-fade_len:] * (1.0 - fade_in)
    
    save_wav(r"Assets\_Project\Audio\sfx_drag_loop.wav", wave_data)

# ============================================================
# 4. SFX: BUTTON PRESS (Snappy, juicy arcade/casual UI click)
# ============================================================
def generate_button_press():
    duration = 0.09
    num_samples = int(SAMPLE_RATE * duration)
    t = np.linspace(0, duration, num_samples, endpoint=False)
    
    # Dual pop: crisp snap at 1600Hz + warm pop at 480Hz
    snap = np.sin(2 * np.pi * 1600 * t) * np.exp(-t / 0.012)
    pop = np.sin(2 * np.pi * 480 * t) * np.exp(-t / 0.035)
    wave_data = snap * 0.6 + pop * 0.8
    save_wav(r"Assets\_Project\Audio\sfx_button_press.wav", wave_data)

# ============================================================
# 5. NEW CUTE BGM (Fresh, uplifting, cozy, cheerful loop)
# ============================================================
def generate_new_cute_bgm():
    BPM = 114.0
    BEAT_SEC = 60.0 / BPM
    BAR_SEC = BEAT_SEC * 4.0
    NUM_BARS = 16
    TOTAL_DURATION = BAR_SEC * NUM_BARS
    TOTAL_SAMPLES = int(SAMPLE_RATE * TOTAL_DURATION)
    
    NOTE_NAMES = ['C', 'C#', 'D', 'D#', 'E', 'F', 'F#', 'G', 'G#', 'A', 'A#', 'B']
    NOTE_FREQS = {}
    for midi in range(12, 108):
        octave = (midi // 12) - 1
        name = NOTE_NAMES[midi % 12] + str(octave)
        NOTE_FREQS[name] = 440.0 * (2.0 ** ((midi - 69) / 12.0))

    left_channel = np.zeros(TOTAL_SAMPLES, dtype=np.float64)
    right_channel = np.zeros(TOTAL_SAMPLES, dtype=np.float64)

    def add_wrapped(audio_l, audio_r, start_sample):
        length = len(audio_l)
        for i in range(length):
            idx = (start_sample + i) % TOTAL_SAMPLES
            left_channel[idx] += audio_l[i]
            right_channel[idx] += audio_r[i]

    # Synthesizers
    def synth_kalimba(freq, dur_sec, pan=0.0, vel=1.0):
        samples = int(SAMPLE_RATE * (dur_sec + 0.6))
        t = np.linspace(0, samples / SAMPLE_RATE, samples, endpoint=False)
        w = np.sin(2 * np.pi * freq * t) + 0.3 * np.sin(2 * np.pi * freq * 2.75 * t) * np.exp(-t/0.06) + 0.15 * np.sin(2 * np.pi * freq * 4.0 * t) * np.exp(-t/0.03)
        env = (1.0 - np.exp(-t / 0.003)) * np.exp(-t / 0.42)
        sig = w * env * (vel * 0.32)
        return sig * ((1.0 - pan) * 0.5), sig * ((1.0 + pan) * 0.5)

    def synth_music_box(freq, dur_sec, pan=0.0, vel=1.0):
        samples = int(SAMPLE_RATE * (dur_sec + 0.8))
        t = np.linspace(0, samples / SAMPLE_RATE, samples, endpoint=False)
        w = np.sin(2 * np.pi * freq * t) + 0.4 * np.sin(2 * np.pi * freq * 2.0 * t) + 0.18 * np.sin(2 * np.pi * freq * 3.0 * t) + 0.08 * np.sin(2 * np.pi * freq * 4.0 * t)
        env = (1.0 - np.exp(-t / 0.004)) * np.exp(-t / 0.55)
        sig = w * env * (vel * 0.26)
        return sig * ((1.0 - pan) * 0.5), sig * ((1.0 + pan) * 0.5)

    def synth_ukulele(freq, dur_sec, pan=0.0, vel=1.0):
        samples = int(SAMPLE_RATE * (dur_sec + 0.4))
        t = np.linspace(0, samples / SAMPLE_RATE, samples, endpoint=False)
        w = np.sin(2 * np.pi * freq * t) + 0.35 * np.sin(2 * np.pi * freq * 2 * t) + 0.15 * np.sin(2 * np.pi * freq * 3 * t)
        env = (1.0 - np.exp(-t / 0.005)) * np.exp(-t / 0.35)
        sig = w * env * (vel * 0.20)
        return sig * ((1.0 - pan) * 0.5), sig * ((1.0 + pan) * 0.5)

    def synth_bass(freq, dur_sec, pan=0.0, vel=1.0):
        samples = int(SAMPLE_RATE * (dur_sec + 0.3))
        t = np.linspace(0, samples / SAMPLE_RATE, samples, endpoint=False)
        w = np.sin(2 * np.pi * freq * t) + 0.4 * np.sin(2 * np.pi * freq * 2.0 * t) * np.exp(-t / 0.15)
        env = (1.0 - np.exp(-t / 0.008)) * np.exp(-t / 0.38)
        sig = w * env * (vel * 0.40)
        return sig * ((1.0 - pan) * 0.5), sig * ((1.0 + pan) * 0.5)

    def synth_shaker(pan=0.2, vel=1.0):
        samples = int(SAMPLE_RATE * 0.06)
        t = np.linspace(0, samples / SAMPLE_RATE, samples, endpoint=False)
        noise = np.diff(np.random.uniform(-1, 1, samples), prepend=0)
        env = (t / 0.012) * np.exp(-t / 0.018)
        sig = noise * env * (vel * 0.10)
        return sig * ((1.0 - pan) * 0.5), sig * ((1.0 + pan) * 0.5)

    def synth_tap(high=True, pan=0.15, vel=1.0):
        f = 1200.0 if high else 850.0
        samples = int(SAMPLE_RATE * 0.1)
        t = np.linspace(0, samples / SAMPLE_RATE, samples, endpoint=False)
        w = np.sin(2 * np.pi * f * t) + 0.25 * np.sin(2 * np.pi * f * 2.1 * t)
        env = np.exp(-t / 0.02) * (1.0 - np.exp(-t / 0.001))
        sig = w * env * (vel * 0.14)
        return sig * ((1.0 - pan) * 0.5), sig * ((1.0 + pan) * 0.5)

    # Key of G major / E minor progression (Bright, sunny, cozy!)
    # Bars 1-4: Gmaj7 -> Cmaj7 -> D7 -> Gmaj7
    # Bars 5-8: Em7 -> Am7 -> D7 -> Gmaj7
    # Bars 9-12: Cmaj7 -> Bm7 -> Am7 -> D7
    # Bars 13-16: Em7 -> A7 -> Dsus4 -> D7
    PROGRESSION = [
        [('G3', 'B3', 'D4', 'F#4'), ('G2', 'D3')],
        [('C3', 'E3', 'G3', 'B3'), ('C2', 'G2')],
        [('D3', 'F#3', 'A3', 'C4'), ('D2', 'A2')],
        [('G3', 'B3', 'D4', 'F#4'), ('G2', 'D3')],
        [('E3', 'G3', 'B3', 'D4'), ('E2', 'B2')],
        [('A3', 'C4', 'E4', 'G4'), ('A2', 'E3')],
        [('D3', 'F#3', 'A3', 'C4'), ('D2', 'A2')],
        [('G3', 'B3', 'D4', 'F#4'), ('G2', 'D3')],
        [('C3', 'E3', 'G3', 'B3'), ('C2', 'G2')],
        [('B2', 'D3', 'F#3', 'A3'), ('B2', 'F#2')],
        [('A2', 'C3', 'E3', 'G3'), ('A2', 'E2')],
        [('D3', 'F#3', 'A3', 'C4'), ('D2', 'A2')],
        [('E3', 'G3', 'B3', 'D4'), ('E2', 'B2')],
        [('A3', 'C#4', 'E4', 'G4'), ('A2', 'E3')],
        [('D3', 'G3', 'A3', 'C4'), ('D2', 'A2')],
        [('D3', 'F#3', 'A3', 'C4'), ('D2', 'F#2')]
    ]

    # Rhythm track
    for bar_idx in range(NUM_BARS):
        bar_start = bar_idx * BAR_SEC
        chord_notes, bass_notes = PROGRESSION[bar_idx]

        # Bass beat 1 & 3
        b1_l, b1_r = synth_bass(NOTE_FREQS[bass_notes[0]], BEAT_SEC * 1.5, vel=0.95)
        b2_l, b2_r = synth_bass(NOTE_FREQS[bass_notes[1]], BEAT_SEC * 1.5, vel=0.85)
        add_wrapped(b1_l, b1_r, int(bar_start * SAMPLE_RATE))
        add_wrapped(b2_l, b2_r, int((bar_start + 2.0 * BEAT_SEC) * SAMPLE_RATE))

        # Ukulele 8th-note bounce
        pat = [0, 1, 2, 3, 1, 2, 3, 2]
        for step in range(8):
            st = bar_start + step * (BEAT_SEC * 0.5)
            note = chord_notes[pat[step] % len(chord_notes)]
            pan = -0.3 if step % 2 == 0 else -0.2
            vl = 0.8 if step % 2 == 0 else 0.6
            uk_l, uk_r = synth_ukulele(NOTE_FREQS[note], BEAT_SEC * 0.8, pan=pan, vel=vl)
            add_wrapped(uk_l, uk_r, int(st * SAMPLE_RATE))

        # Percussion: woodblock on 2 and 4
        t1_l, t1_r = synth_tap(high=False, pan=0.15, vel=0.75)
        t2_l, t2_r = synth_tap(high=True, pan=0.25, vel=0.85)
        add_wrapped(t1_l, t1_r, int((bar_start + 1.0 * BEAT_SEC) * SAMPLE_RATE))
        add_wrapped(t2_l, t2_r, int((bar_start + 3.0 * BEAT_SEC) * SAMPLE_RATE))

        # 16th shaker
        for s in range(16):
            sh_t = bar_start + s * (BEAT_SEC * 0.25)
            acc = 0.8 if (s % 4 == 2) else (0.5 if (s % 2 == 0) else 0.3)
            sh_l, sh_r = synth_shaker(pan=0.25, vel=acc)
            add_wrapped(sh_l, sh_r, int(sh_t * SAMPLE_RATE))

    # Catchy, bouncy, cute sunny melody (Glockenspiel + Kalimba)
    MELODY = [
        # Phrase 1: Bars 0-3 (Happy skip: D5 -> G5 -> B5 -> A5...)
        (0, 0.0, 'D5', 0.5, 0.9),
        (0, 0.5, 'G5', 0.75, 1.05),
        (0, 1.5, 'B5', 0.75, 1.1),
        (0, 2.5, 'A5', 0.5, 0.85),
        (0, 3.0, 'G5', 1.0, 1.0),

        (1, 0.0, 'E5', 0.75, 0.95),
        (1, 1.0, 'G5', 0.5, 0.85),
        (1, 1.5, 'C6', 1.0, 1.1),
        (1, 3.0, 'B5', 1.0, 0.9),

        (2, 0.0, 'A5', 0.5, 0.9),
        (2, 0.5, 'B5', 0.5, 0.95),
        (2, 1.0, 'C6', 1.0, 1.0),
        (2, 2.0, 'D6', 1.0, 1.1),
        (2, 3.0, 'A5', 1.0, 0.85),

        (3, 0.0, 'B5', 1.5, 1.05),
        (3, 2.0, 'G5', 1.5, 0.95),

        # Phrase 2: Bars 4-7 (Playful response)
        (4, 0.0, 'G5', 0.5, 0.9),
        (4, 0.5, 'B5', 0.75, 1.0),
        (4, 1.5, 'E6', 1.0, 1.15),
        (4, 3.0, 'D6', 1.0, 0.95),

        (5, 0.0, 'C6', 0.75, 1.0),
        (5, 1.0, 'B5', 0.5, 0.85),
        (5, 1.5, 'A5', 1.0, 0.9),
        (5, 3.0, 'G5', 1.0, 0.85),

        (6, 0.0, 'F#5', 0.5, 0.85),
        (6, 0.5, 'A5', 0.5, 0.9),
        (6, 1.0, 'D6', 1.5, 1.1),
        (6, 3.0, 'C6', 1.0, 0.9),

        (7, 0.0, 'B5', 2.0, 1.05),
        (7, 2.5, 'A5', 0.5, 0.8),
        (7, 3.0, 'B5', 0.5, 0.85),
        (7, 3.5, 'C6', 0.5, 0.9),

        # Phrase 3: Bars 8-11 (Sweet bell climax)
        (8, 0.0, 'D6', 1.0, 1.15),
        (8, 1.0, 'E6', 1.0, 1.2),
        (8, 2.0, 'G6', 1.5, 1.25),
        (8, 3.5, 'E6', 0.5, 0.95),

        (9, 0.0, 'D6', 1.5, 1.1),
        (9, 2.0, 'B5', 1.5, 1.0),

        (10, 0.0, 'C6', 1.0, 1.05),
        (10, 1.0, 'D6', 0.75, 1.0),
        (10, 2.0, 'E6', 1.5, 1.1),
        (10, 3.5, 'C6', 0.5, 0.85),

        (11, 0.0, 'D6', 2.0, 1.1),
        (11, 2.5, 'D5', 0.5, 0.85),
        (11, 3.0, 'E5', 0.5, 0.85),
        (11, 3.5, 'F#5', 0.5, 0.9),

        # Phrase 4: Bars 12-15 (Resolution leading seamlessly into loop)
        (12, 0.0, 'G5', 1.0, 1.05),
        (12, 1.0, 'B5', 1.0, 1.1),
        (12, 2.0, 'D6', 1.5, 1.15),

        (13, 0.0, 'E6', 1.0, 1.1),
        (13, 1.0, 'C#6', 1.0, 1.0),
        (13, 2.0, 'A5', 1.5, 0.95),

        (14, 0.0, 'D6', 1.5, 1.1),
        (14, 2.0, 'G5', 1.0, 0.95),
        (14, 3.0, 'A5', 1.0, 1.0),

        (15, 0.0, 'B5', 1.0, 1.05),
        (15, 1.0, 'A5', 1.0, 0.95),
        (15, 2.0, 'G5', 1.0, 0.9),
        (15, 3.0, 'F#5', 0.5, 0.85),
        (15, 3.5, 'A5', 0.5, 0.9)
    ]

    for bar, beat, note, dur, vel in MELODY:
        t_sec = bar * BAR_SEC + beat * BEAT_SEC
        mb_l, mb_r = synth_music_box(NOTE_FREQS[note], dur * BEAT_SEC, pan=0.12, vel=vel)
        add_wrapped(mb_l, mb_r, int(t_sec * SAMPLE_RATE))
        kl_l, kl_r = synth_kalimba(NOTE_FREQS[note], dur * BEAT_SEC * 0.9, pan=-0.12, vel=vel * 0.7)
        add_wrapped(kl_l, kl_r, int(t_sec * SAMPLE_RATE))

    save_wav(r"Assets\_Project\Audio\bgm_cute_loop.wav", left_channel, right_channel)

if __name__ == "__main__":
    generate_pick_box()
    generate_drop_box()
    generate_drag_loop()
    generate_button_press()
    generate_new_cute_bgm()
    print("All audio assets generated successfully!")
