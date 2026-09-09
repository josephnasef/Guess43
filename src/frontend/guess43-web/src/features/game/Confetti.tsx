import { useEffect, useRef } from 'react';

/** Lightweight confetti burst that respects the user's reduced-motion preference. */
export function Confetti({ active }: { active: boolean }) {
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    if (!active) return;
    const prefersReduced = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches;
    if (prefersReduced) return;

    const canvas = canvasRef.current;
    if (!canvas) return;
    let ctx: CanvasRenderingContext2D | null = null;
    try {
      ctx = canvas.getContext('2d');
    } catch {
      // Canvas 2D context unavailable (e.g. jsdom); silently skip the effect.
      return;
    }
    if (!ctx) return;

    canvas.width = canvas.offsetWidth;
    canvas.height = canvas.offsetHeight;
    const colors = ['#6366f1', '#22c55e', '#f59e0b', '#ec4899', '#38bdf8'];
    const pieces = Array.from({ length: 120 }, () => ({
      x: Math.random() * canvas.width,
      y: -20 - Math.random() * canvas.height,
      size: 4 + Math.random() * 6,
      speed: 2 + Math.random() * 3,
      drift: -1 + Math.random() * 2,
      color: colors[Math.floor(Math.random() * colors.length)],
    }));

    let frame = 0;
    let raf = 0;
    const render = () => {
      frame += 1;
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      for (const p of pieces) {
        p.y += p.speed;
        p.x += p.drift;
        ctx.fillStyle = p.color;
        ctx.fillRect(p.x, p.y, p.size, p.size);
      }
      if (frame < 160) {
        raf = requestAnimationFrame(render);
      } else {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
      }
    };
    raf = requestAnimationFrame(render);
    return () => cancelAnimationFrame(raf);
  }, [active]);

  if (!active) return null;
  return <canvas ref={canvasRef} className="confetti" aria-hidden="true" />;
}
