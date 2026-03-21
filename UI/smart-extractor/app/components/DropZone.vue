<template>
  <div class="relative group">
    <div class="absolute -inset-1 bg-gradient-to-r from-primary/20 to-secondary/20 rounded-xl blur opacity-25 group-hover:opacity-50 transition duration-1000 group-hover:duration-200" />
    <div class="relative flex flex-col items-center justify-center border-2 border-dashed border-outline-variant/30 bg-surface-container-low rounded-xl p-12 transition-all duration-300 hover:border-primary/40">
      <div class="w-16 h-16 rounded-full bg-surface-container-high flex items-center justify-center mb-4 neon-glow-primary">
        <span class="material-symbols-outlined text-primary text-3xl">upload_file</span>
      </div>
      <h2 class="font-headline font-bold text-xl mb-2 text-on-surface">
        Drop your PDF
      </h2>
      <p class="font-body text-on-surface-variant text-sm mb-6 text-center max-w-xs">
        Drag and drop your financial reports, invoices, or research papers for instant extraction.
      </p>
      <button class="bg-gradient-to-br from-primary to-primary-dim text-on-primary-fixed font-headline font-bold px-8 py-3 rounded-xl transition-all active:scale-95 shadow-lg shadow-primary/20">
        Upload File
      </button>
    </div>
  </div>
</template>
