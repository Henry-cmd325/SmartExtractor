<template>
  <aside class="fixed left-0 top-0 h-screen w-20 flex flex-col items-center py-8 z-40 bg-surface-container-low border-r border-outline-variant/15 shadow-[4px_0_24px_rgba(0,0,0,0.5)]">
    <div class="mb-12">
      <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-primary to-primary-dim flex items-center justify-center shadow-lg shadow-primary/20">
        <span
          class="material-symbols-outlined text-on-primary-fixed"
          style="font-variation-settings: 'FILL' 1;"
        >bolt</span>
      </div>
    </div>
    <nav class="flex flex-col gap-8 flex-1">
      <!-- Extract Active -->
      <button class="group relative flex flex-col items-center gap-1 bg-surface-container-high text-secondary shadow-[0_0_15px_rgba(83,221,252,0.1)] rounded-xl p-3 transition-all duration-200 active:scale-90">
        <span class="material-symbols-outlined">terminal</span>
        <span class="font-label text-[10px] uppercase tracking-widest">Extract</span>
      </button>
      <button class="group flex flex-col items-center gap-1 text-on-surface-variant hover:text-primary hover:bg-surface-container-high/50 p-3 rounded-xl transition-all active:scale-90">
        <span class="material-symbols-outlined">folder_open</span>
        <span class="font-label text-[10px] uppercase tracking-widest">Library</span>
      </button>
      <button class="group flex flex-col items-center gap-1 text-on-surface-variant hover:text-primary hover:bg-surface-container-high/50 p-3 rounded-xl transition-all active:scale-90">
        <span class="material-symbols-outlined">psychology</span>
        <span class="font-label text-[10px] uppercase tracking-widest">Models</span>
      </button>
      <button class="group flex flex-col items-center gap-1 text-on-surface-variant hover:text-primary hover:bg-surface-container-high/50 p-3 rounded-xl transition-all active:scale-90">
        <span class="material-symbols-outlined">code</span>
        <span class="font-label text-[10px] uppercase tracking-widest">API</span>
      </button>
    </nav>
    <div class="flex flex-col gap-6 mt-auto">
      <button class="group flex flex-col items-center gap-1 text-on-surface-variant hover:text-primary transition-all">
        <span class="material-symbols-outlined">help</span>
        <span class="font-label text-[10px] uppercase tracking-widest">Help</span>
      </button>
      <button class="group flex flex-col items-center gap-1 text-on-surface-variant hover:text-primary transition-all">
        <span class="material-symbols-outlined">logout</span>
        <span class="font-label text-[10px] uppercase tracking-widest">Logout</span>
      </button>
    </div>
  </aside>
</template>
