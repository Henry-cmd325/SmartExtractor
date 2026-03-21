<template>
  <section class="w-[450px] bg-surface-container-low border-l border-outline-variant/15 flex flex-col">
    <div class="p-6 flex items-center justify-between border-b border-outline-variant/10">
      <div class="flex items-center gap-3">
        <span class="material-symbols-outlined text-primary">visibility</span>
        <h3 class="font-headline font-bold text-sm uppercase tracking-wider text-on-surface">
          Live PDF Preview
        </h3>
      </div>
      <div class="flex items-center gap-2">
        <button class="p-1 hover:bg-surface-container-high rounded transition-colors text-on-surface">
          <span class="material-symbols-outlined text-lg">zoom_in</span>
        </button>
        <button class="p-1 hover:bg-surface-container-high rounded transition-colors text-on-surface">
          <span class="material-symbols-outlined text-lg">zoom_out</span>
        </button>
      </div>
    </div>
    <div class="flex-1 overflow-y-auto p-8 bg-surface-container-lowest flex justify-center items-start">
      <!-- Simulated PDF Page -->
      <div class="w-full max-w-sm aspect-[1/1.4] bg-white rounded-sm shadow-2xl relative overflow-hidden flex flex-col">
        <div class="p-6 space-y-4">
          <div class="h-6 w-1/3 bg-slate-200 rounded" />
          <div class="h-4 w-full bg-slate-100 rounded" />
          <div class="h-4 w-5/6 bg-slate-100 rounded" />
          <!-- Highlighted Table Area in PDF -->
          <div class="mt-8 border-2 border-primary/40 bg-primary/5 rounded relative">
            <div class="absolute -top-3 -right-3 bg-primary text-white text-[8px] px-2 py-1 rounded-full font-bold">
              EXTRACTING...
            </div>
            <div class="p-4 space-y-3">
              <div class="flex gap-2">
                <div class="h-3 w-1/4 bg-slate-300 rounded" />
                <div class="h-3 w-1/2 bg-slate-300 rounded" />
                <div class="h-3 w-1/4 bg-slate-300 rounded" />
              </div>
              <div class="flex gap-2">
                <div class="h-2 w-1/4 bg-slate-200 rounded" />
                <div class="h-2 w-1/2 bg-slate-200 rounded" />
                <div class="h-2 w-1/4 bg-slate-200 rounded" />
              </div>
              <div class="flex gap-2">
                <div class="h-2 w-1/4 bg-slate-200 rounded" />
                <div class="h-2 w-1/2 bg-slate-200 rounded" />
                <div class="h-2 w-1/4 bg-slate-200 rounded" />
              </div>
            </div>
          </div>
          <div class="mt-8 space-y-4">
            <div class="h-4 w-full bg-slate-100 rounded" />
            <div class="h-4 w-4/5 bg-slate-100 rounded" />
            <div class="h-4 w-full bg-slate-100 rounded" />
          </div>
        </div>
        <!-- PDF Page Texture/Description -->
        <img
          alt="Document Texture"
          class="absolute inset-0 opacity-10 pointer-events-none"
          src="https://lh3.googleusercontent.com/aida-public/AB6AXuD_CcsLW7XABbNNS63HRJ3hAW5beymZDvKyDvPDj7RSR0ix8mGdbL87DH4tOCHbqf5MaFR8sql88TYceZUj_j4KiZH-nPImf4dkmSLGoZyBsTWeSN1uhtu-58ANRSvjGNULB_naOFv54pEOzasXCMbHjNAm-X-orw_Bo905cKlP9mdTLY-mEhejn3A-4daNaaXPLlj9XFmz8dEZ4g5TJYLSKjueRw982EmJkL8nj5IwfY75Ok73SbhJLlC1BXTkmYWChZyigEnq4cuG"
        >
      </div>
    </div>
    <div class="p-6 bg-surface-container-high border-t border-outline-variant/10">
      <div class="flex justify-between items-center mb-4">
        <span class="font-label text-[10px] text-on-surface-variant">PAGE 1 OF 12</span>
        <div class="flex gap-1 text-on-surface">
          <button class="w-8 h-8 rounded border border-outline-variant/20 flex items-center justify-center hover:bg-surface-variant">
            <span class="material-symbols-outlined text-sm">chevron_left</span>
          </button>
          <button class="w-8 h-8 rounded border border-outline-variant/20 flex items-center justify-center hover:bg-surface-variant">
            <span class="material-symbols-outlined text-sm">chevron_right</span>
          </button>
        </div>
      </div>
      <div class="w-full bg-surface-container-lowest h-1.5 rounded-full overflow-hidden">
        <div class="h-full bg-primary-dim w-[8.33%]" />
      </div>
    </div>
  </section>
</template>
