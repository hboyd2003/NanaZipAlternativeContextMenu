separator
$trimmedName = if(str.length(sel.title) > 10, str.sub(sel.title, 0, 10) + '...', sel.title)
$fileName = if(sel.count == 1, trimmedName, "*")
//Extract
item(type='file'
	 title='Extract to .\@fileName'
	 pos='middle'
	 mode='multiple'
	 image='@app.directory\\Icons\\NanaZipSfx.ico'
	 find='.001|.7z|.apfs|.arj|.bz2|.bzip2|.cab|.cpio|.deb|.dmg|.esd|.fat|.gzip|.hfs|.hfsx|.iso|.lha|.liz|.lz|.lz4|.lz5|.lzh|.lzma|.ntfs|.rar|.rpm|.squashfs|.swm|.tar|.taz|.tbz|.tbz2|.tgz|.tlz|.tpz|.txz|.vhd|.vhdx|.wim|.xar|.xz|.z|.zip|.zst|.gz'
	 cmd='@app.directory\\Tools\\NanaZipContext.exe'
	 args='"decompress"  @sel(true, ',')')

//Extract and Recycle
item(type='file'
	 title='Extract to .\@fileName and Recycle'
	 pos='middle'
	 mode='multiple'
	 image='@app.directory\\Icons\\NanaZipSfx.ico'
	 find='.001|.7z|.apfs|.arj|.bz2|.bzip2|.cab|.cpio|.deb|.dmg|.esd|.fat|.gzip|.hfs|.hfsx|.iso|.lha|.liz|.lz|.lz4|.lz5|.lzh|.lzma|.ntfs|.rar|.rpm|.squashfs|.swm|.tar|.taz|.tbz|.tbz2|.tgz|.tlz|.tpz|.txz|.vhd|.vhdx|.wim|.xar|.xz|.z|.zip|.zst|.gz'
	 cmd='@app.directory\\Tools\\NanaZipContext.exe'
	 args='"decompressAndRecycle"  @sel(true, ',')')


//Add to Archive
item(type='file|directory'
	 title='Add to archive'
	 pos='middle'
	 mode='multiple'
	 image='@app.directory\\Icons\\NanaZip.ico'
	 cmd='@app.directory\\Tools\\NanaZipContext.exe'
	 args='"addToArchive"  @sel(true, ',')')


item(type='directory'
	 title='Create archive from folder'
	 pos='middle'
	 mode='single'
	 image='@app.directory\\Icons\\NanaZip.ico'
	 cmd='@app.directory\\Tools\\NanaZipContext.exe'
	 args='"addToArchive"  @sel(true, ',')')
separator
