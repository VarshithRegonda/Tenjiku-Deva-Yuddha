import { Platform } from 'react-native';

export function injectGlobalCSS() {
  if (Platform.OS !== 'web' || typeof document === 'undefined') return () => {};
  const ID = 'tdv-global-css';
  if (document.getElementById(ID)) return () => {};

  const s = document.createElement('style');
  s.id = ID;
  s.innerHTML = `
    @import url('https://fonts.googleapis.com/css2?family=Cinzel:wght@400;700;900&family=Rajdhani:wght@400;500;600;700&family=Inter:wght@300;400;500;600;700&display=swap');

    *, *::before, *::after { box-sizing: border-box; }

    body, #root { background: #020408 !important; }

    ::-webkit-scrollbar { width: 4px; height: 4px; }
    ::-webkit-scrollbar-track { background: #0D1117; }
    ::-webkit-scrollbar-thumb { background: rgba(0,240,255,0.3); border-radius: 2px; }
    ::-webkit-scrollbar-thumb:hover { background: rgba(0,240,255,0.6); }

    /* ── Keyframes ── */
    @keyframes tdvGodRay {
      0%   { transform: rotate(0deg)   scale(1);   opacity: .03; }
      50%  { opacity: .07; }
      100% { transform: rotate(360deg) scale(1.05); opacity: .03; }
    }
    @keyframes tdvChakraSpin    { to { transform: rotate(360deg); } }
    @keyframes tdvChakraCounter { to { transform: rotate(-360deg); } }
    @keyframes tdvLotusPulse {
      0%,100% { transform: scale(1)    rotate(0deg);   opacity: .04; }
      50%     { transform: scale(1.15) rotate(180deg); opacity: .10; }
    }
    @keyframes tdvParticleRise {
      0%   { transform: translateY(0)       scale(0);   opacity: 0; }
      8%   { opacity: 1; }
      92%  { opacity: .6; }
      100% { transform: translateY(-100vh) translateX(var(--drift,20px)) scale(1.5); opacity: 0; }
    }
    @keyframes tdvGlyphDrift {
      0%   { transform: translateY(0)    rotate(0deg);  opacity: 0; }
      12%  { opacity: .22; }
      88%  { opacity: .10; }
      100% { transform: translateY(-80px) rotate(10deg); opacity: 0; }
    }
    @keyframes tdvScanLine {
      0%   { top: -3px; opacity: 0; }
      5%   { opacity: 1; }
      95%  { opacity: 1; }
      100% { top: 100%; opacity: 0; }
    }
    @keyframes tdvBattleFlash { 0%{opacity:0} 12%{opacity:.8} 100%{opacity:0} }
    @keyframes tdvEmberRise {
      0%   { transform: translateY(0)      scale(1);    opacity: 1; }
      100% { transform: translateY(-130px) translateX(var(--ex,8px)) scale(.05); opacity: 0; }
    }
    @keyframes tdvGoldShimmer {
      0%   { background-position: -250% center; }
      100% { background-position:  250% center; }
    }
    @keyframes tdvPulseGlow {
      0%,100% { box-shadow: 0 0 15px rgba(0,240,255,.12), 0 0 4px rgba(0,240,255,.06); }
      50%     { box-shadow: 0 0 40px rgba(0,240,255,.35), 0 0 12px rgba(0,240,255,.15); }
    }
    @keyframes tdvBattleThrob {
      0%,100% { box-shadow: 0 0 25px rgba(255,68,68,.5);  transform: scale(1);     }
      50%     { box-shadow: 0 0 55px rgba(255,68,68,.9), 0 0 90px rgba(255,68,68,.2); transform: scale(1.018); }
    }
    @keyframes tdvAuraBreathe {
      0%,100% { box-shadow: 0 0 20px rgba(0,240,255,.10), inset 0 0 20px rgba(0,240,255,.03); }
      50%     { box-shadow: 0 0 50px rgba(0,240,255,.28), inset 0 0 30px rgba(0,240,255,.08); }
    }
    @keyframes tdvSlideIn {
      from { transform: translateY(12px); opacity: 0; }
      to   { transform: translateY(0);    opacity: 1; }
    }
    @keyframes tdvFadeIn {
      from { opacity: 0; } to { opacity: 1; }
    }
    @keyframes tdvTickerScroll {
      0%   { transform: translateX(100vw); }
      100% { transform: translateX(-100%); }
    }
    @keyframes tdvRotateOrb {
      from { transform: rotate(0deg); }
      to   { transform: rotate(360deg); }
    }

    /* ── VFX Layer ── */
    .tdv-vfx-layer  { position:fixed; inset:0; pointer-events:none; z-index:0; overflow:hidden; }
    .tdv-god-ray    {
      position:absolute; top:50%; left:50%;
      width:230vmax; height:230vmax;
      margin:-115vmax 0 0 -115vmax;
      background: conic-gradient(
        from 0deg,
        transparent 0deg, rgba(255,215,0,.025) 2deg, transparent 5deg,
        transparent 45deg, rgba(0,240,255,.02) 47deg, transparent 50deg,
        transparent 90deg, rgba(255,215,0,.03) 92deg, transparent 95deg,
        transparent 135deg, rgba(0,240,255,.018) 137deg, transparent 140deg,
        transparent 180deg, rgba(255,215,0,.025) 182deg, transparent 185deg,
        transparent 225deg, rgba(0,240,255,.02) 227deg, transparent 230deg,
        transparent 270deg, rgba(255,215,0,.03) 272deg, transparent 275deg,
        transparent 315deg, rgba(0,240,255,.018) 317deg, transparent 320deg
      );
      animation: tdvGodRay 90s linear infinite;
    }
    .tdv-lotus-bg   {
      position:absolute; top:50%; left:50%;
      width:90vmin; height:90vmin; margin:-45vmin 0 0 -45vmin;
      background: radial-gradient(ellipse at center, rgba(255,215,0,.035) 0%, rgba(0,240,255,.02) 40%, transparent 70%);
      animation: tdvLotusPulse 12s ease-in-out infinite;
    }
    .tdv-chakra-ring   { position:absolute; top:50%; left:50%; border-radius:50%; border-style:solid; border-color:transparent; }
    .tdv-chakra-outer  { width:88vmin; height:88vmin; margin:-44vmin 0 0 -44vmin; border-width:1px; border-top-color:rgba(255,215,0,.06); border-right-color:rgba(0,240,255,.04); animation:tdvChakraSpin 40s linear infinite; }
    .tdv-chakra-mid    { width:66vmin; height:66vmin; margin:-33vmin 0 0 -33vmin; border-width:1px; border-top-color:rgba(0,240,255,.08); border-left-color:rgba(255,68,68,.04);  animation:tdvChakraCounter 26s linear infinite; }
    .tdv-chakra-inner  { width:44vmin; height:44vmin; margin:-22vmin 0 0 -22vmin; border-width:1px; border-top-color:rgba(255,215,0,.10); border-right-color:rgba(0,240,255,.06); animation:tdvChakraSpin 16s linear infinite; }
    .tdv-particle { position:absolute; bottom:0; border-radius:50%; animation:tdvParticleRise linear infinite; }
    .tdv-glyph    { position:absolute; font-family:'Noto Sans Devanagari',sans-serif; color:rgba(255,215,0,.18); animation:tdvGlyphDrift ease-in-out infinite; pointer-events:none; user-select:none; }

    /* ── HUD ── */
    .tdv-hud-scan {
      position:fixed; left:0; right:0; height:2px; pointer-events:none; z-index:9990;
      background: linear-gradient(90deg, transparent, rgba(0,240,255,.45), rgba(255,215,0,.25), rgba(0,240,255,.45), transparent);
      animation: tdvScanLine 10s linear infinite;
    }

    /* ── Battle FX ── */
    .tdv-battle-flash     { position:fixed; inset:0; pointer-events:none; z-index:99998; animation:tdvBattleFlash .7s ease-out forwards; }
    .tdv-battle-flash.win { background:rgba(0,255,136,.22); }
    .tdv-battle-flash.loss{ background:rgba(255,68,68,.28); }
    .tdv-ember { position:fixed; width:5px; height:5px; border-radius:50%; animation:tdvEmberRise ease-out forwards; }

    /* ── Utility Classes ── */
    .tdv-gold-text {
      background: linear-gradient(135deg, #B8860B 0%, #FFD700 35%, #FFFACD 55%, #FFD700 70%, #B8860B 100%);
      background-size: 250% 100%;
      -webkit-background-clip: text; background-clip: text; -webkit-text-fill-color: transparent;
      animation: tdvGoldShimmer 4s linear infinite;
    }
    .tdv-cyan-text { color: #00F0FF; text-shadow: 0 0 20px rgba(0,240,255,.6); }
    .tdv-battle-throb { animation: tdvBattleThrob 2.4s ease-in-out infinite; }
    .tdv-aura-breathe { animation: tdvAuraBreathe 3.5s ease-in-out infinite; }
    .tdv-pulse-glow   { animation: tdvPulseGlow   3s ease-in-out infinite; }
    .tdv-slide-in     { animation: tdvSlideIn .35s ease-out both; }
    .tdv-fade-in      { animation: tdvFadeIn  .25s ease-out both; }

    /* ── Glassmorphism Cards ── */
    .tdv-glass {
      background: rgba(13,17,23,0.80);
      backdrop-filter: blur(20px) saturate(180%);
      -webkit-backdrop-filter: blur(20px) saturate(180%);
      border: 1px solid rgba(255,255,255,0.06);
    }
    .tdv-glass-gold {
      background: rgba(13,17,23,0.85);
      backdrop-filter: blur(20px) saturate(180%);
      border: 1px solid rgba(255,215,0,0.18);
      box-shadow: 0 0 30px rgba(255,215,0,0.06), inset 0 1px 0 rgba(255,215,0,0.08);
    }
    .tdv-glass-cyan {
      background: rgba(13,17,23,0.85);
      backdrop-filter: blur(20px) saturate(180%);
      border: 1px solid rgba(0,240,255,0.18);
      box-shadow: 0 0 30px rgba(0,240,255,0.06), inset 0 1px 0 rgba(0,240,255,0.08);
    }
    .tdv-glass-red {
      background: rgba(13,17,23,0.85);
      backdrop-filter: blur(20px) saturate(180%);
      border: 1px solid rgba(255,68,68,0.25);
      box-shadow: 0 0 30px rgba(255,68,68,0.08), inset 0 1px 0 rgba(255,68,68,0.10);
    }

    /* ── Hover Effects (web only) ── */
    .tdv-nav-btn {
      transition: all 0.2s ease;
      cursor: pointer;
    }
    .tdv-nav-btn:hover { background: rgba(0,240,255,0.06) !important; }
    .tdv-nav-btn.active { background: rgba(0,240,255,0.10) !important; border-left: 2px solid #00F0FF !important; }
    .tdv-card-hover { transition: transform 0.2s ease, box-shadow 0.2s ease; cursor: pointer; }
    .tdv-card-hover:hover { transform: translateY(-2px); box-shadow: 0 8px 40px rgba(0,240,255,0.12) !important; }
    .tdv-btn-primary { transition: all 0.2s ease; cursor: pointer; }
    .tdv-btn-primary:hover { filter: brightness(1.15); transform: scale(1.02); }
    .tdv-btn-primary:active { transform: scale(0.97); }

    /* ── Ticker ── */
    .tdv-ticker-wrap { overflow: hidden; white-space: nowrap; }
    .tdv-ticker-inner { display: inline-block; animation: tdvTickerScroll 28s linear infinite; }

    /* ── Custom Font Classes ── */
    .tdv-font-heading { font-family: 'Cinzel', 'Georgia', serif !important; }
    .tdv-font-ui      { font-family: 'Rajdhani', sans-serif !important; letter-spacing: 0.05em; }
    .tdv-font-body    { font-family: 'Inter', sans-serif !important; }
  `;
  document.head.appendChild(s);

  // VFX DOM Layer
  const layer = document.createElement('div');
  layer.className = 'tdv-vfx-layer';
  layer.id = 'tdv-main-layer';

  const godRay = document.createElement('div'); godRay.className = 'tdv-god-ray'; layer.appendChild(godRay);
  const lotus  = document.createElement('div'); lotus.className  = 'tdv-lotus-bg'; layer.appendChild(lotus);
  ['tdv-chakra-ring tdv-chakra-outer','tdv-chakra-ring tdv-chakra-mid','tdv-chakra-ring tdv-chakra-inner'].forEach(c => {
    const d = document.createElement('div'); d.className = c; layer.appendChild(d);
  });

  const particles = [
    {l:'5%',  sz:3, col:'#FFD700', dur:9,  delay:0, drift:'18px'},
    {l:'18%', sz:2, col:'#00F0FF', dur:13, delay:3, drift:'-24px'},
    {l:'33%', sz:4, col:'#FFD700', dur:10, delay:5, drift:'16px'},
    {l:'48%', sz:2, col:'#FF4444', dur:16, delay:1, drift:'-20px'},
    {l:'62%', sz:3, col:'#00F0FF', dur:11, delay:4, drift:'28px'},
    {l:'77%', sz:2, col:'#FFD700', dur:14, delay:2, drift:'-16px'},
    {l:'90%', sz:3, col:'#00FF88', dur:8,  delay:6, drift:'22px'},
    {l:'25%', sz:2, col:'#00F0FF', dur:12, delay:7, drift:'-30px'},
    {l:'55%', sz:3, col:'#FFD700', dur:17, delay:0, drift:'10px'},
    {l:'70%', sz:4, col:'#00F0FF', dur:9,  delay:3, drift:'-18px'},
  ];
  particles.forEach(p => {
    const el = document.createElement('div');
    el.className = 'tdv-particle';
    el.style.cssText = `left:${p.l};width:${p.sz}px;height:${p.sz}px;background:${p.col};box-shadow:0 0 ${p.sz*3}px ${p.col};animation-duration:${p.dur}s;animation-delay:-${p.delay}s;--drift:${p.drift}`;
    layer.appendChild(el);
  });

  const glyphs = ['ॐ','ऋ','श्री','धर्म','ॐ','वेद','॥','शक्ति','युद्ध','ॐ'];
  glyphs.forEach((g, i) => {
    const el = document.createElement('div');
    el.className = 'tdv-glyph';
    el.textContent = g;
    el.style.cssText = `left:${5+(i*10)%90}%;bottom:${8+(i*17)%65}%;font-size:${12+(i*3)%14}px;animation-duration:${6+(i*2.1)%9}s;animation-delay:-${(i*1.9)%7}s;opacity:0`;
    layer.appendChild(el);
  });

  document.body.insertBefore(layer, document.body.firstChild);

  const scan = document.createElement('div');
  scan.className = 'tdv-hud-scan';
  scan.id = 'tdv-hud-scan';
  document.body.appendChild(scan);

  return () => { layer.remove(); scan.remove(); };
}

export function triggerBattleVFX(result: 'win' | 'loss') {
  if (typeof document === 'undefined') return;
  const flash = document.createElement('div');
  flash.className = `tdv-battle-flash ${result}`;
  document.body.appendChild(flash);
  setTimeout(() => flash.remove(), 750);

  if (result === 'win') {
    for (let i = 0; i < 24; i++) {
      const e = document.createElement('div');
      e.className = 'tdv-ember';
      const col = Math.random() > 0.5 ? '#FFD700' : '#00FF88';
      e.style.cssText = `left:${20+Math.random()*60}%;bottom:${20+Math.random()*30}%;background:${col};box-shadow:0 0 8px ${col};animation-duration:${0.7+Math.random()*0.9}s;animation-delay:${Math.random()*0.25}s;--ex:${(Math.random()-0.5)*60}px`;
      document.body.appendChild(e);
      setTimeout(() => e.remove(), 1800);
    }
  }
}
