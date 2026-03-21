<template>
  <div class="space-y-3">
    <div class="flex items-center gap-2 px-1">
      <span class="material-symbols-outlined text-secondary text-sm">terminal</span>
      <span class="font-label text-[10px] uppercase tracking-widest text-secondary">Extraction Logic Prompt</span>
    </div>
    <div class="bg-surface-container-lowest border border-outline-variant/10 rounded-xl p-4 shadow-inner">
      <div class="flex gap-3">
        <span class="font-label text-primary-dim">VOID@TURBO:~$</span>
        <div class="flex-1">
          <span class="font-label text-secondary block mb-1 italic opacity-70">Tell the AI how to extract your data...</span>
          <div class="flex items-center">
            <span class="font-label text-on-surface text-sm">extract all table data from page 2-4 and format as JSON entities</span>
            <span class="terminal-cursor" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
