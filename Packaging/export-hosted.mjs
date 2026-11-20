import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { execFileSync } from 'node:child_process';
import { createHash } from 'node:crypto';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const output=path.resolve(process.argv[2]||'');
if(!process.argv[2])throw new Error('Pass a fresh export directory.');
await fs.mkdir(output);
const staging=path.join(output,'package');await fs.mkdir(staging);
const sourceRoot='GGUI/Assets/Scripts/GGUI';
const map=[['Packaging/README-hosted.md','README.md'],['Packaging/hosted-package.json','package.json'],['Packaging/GGFramework.GGUI.Runtime.asmdef','Runtime/GGFramework.GGUI.Runtime.asmdef']];
async function collect(relative,destination){
 for(const entry of await fs.readdir(path.join(root,relative),{withFileTypes:true})){
  if(entry.isSymbolicLink())throw new Error('Unexpected source link');
  const from=relative+'/'+entry.name,to=destination+'/'+entry.name;
  if(entry.isDirectory())await collect(from,to);else if(entry.isFile())map.push([from,to]);
 }
}
for(const file of ['UIBase.cs','UIBase.cs.meta','UIWindow.cs','UIWindow.cs.meta'])map.push([sourceRoot+'/'+file,'Runtime/'+file]);
for(const [from,to] of [['GGUI/Assets/Scripts/GGUI.meta','Runtime.meta'],['GGUI/Assets/Scripts/GGUI/Abstraction.meta','Runtime/Abstraction.meta'],['GGUI/Assets/Scripts/GGUI/Hosting.meta','Runtime/Hosting.meta'],['GGUI/Assets/Scripts/GGUI/Implementation.meta','Runtime/Implementation.meta'],['GGUI/Assets/Scripts/GGUI/Implementation/UGUI.meta','Runtime/Implementation/UGUI.meta'],['Packaging/GGFramework.GGUI.Runtime.asmdef.meta','Runtime/GGFramework.GGUI.Runtime.asmdef.meta'],['Packaging/README-hosted.md.meta','README.md.meta']])map.push([from,to]);
for(const dir of ['Abstraction','Hosting','Implementation/UGUI'])await collect(sourceRoot+'/'+dir,'Runtime/'+dir);
const rows=[];
for(const [from,to]of map.sort((a,b)=>a[1]<b[1]?-1:a[1]>b[1]?1:0)){
 const bytes=await fs.readFile(path.join(root,from));await fs.mkdir(path.dirname(path.join(staging,to)),{recursive:true});await fs.writeFile(path.join(staging,to),bytes,{flag:'wx'});
 rows.push({source:from,path:to,bytes:bytes.length,sha256:createHash('sha256').update(bytes).digest('hex')});
}
const pkg=JSON.parse(await fs.readFile(path.join(staging,'package.json'),'utf8'));
const archive='com.gonglei.ggframework.ggui.ugui-'+pkg.version+'.tgz';
execFileSync('tar',['-czf',path.join(output,archive),'-C',output,'package'],{windowsHide:true});
const manifestSha256=createHash('sha256').update(rows.map(r=>r.path+' '+r.sha256).join('\n')).digest('hex');
const receipt={schemaVersion:1,version:pkg.version,baseCommit:execFileSync('git',['rev-parse','HEAD'],{cwd:root,encoding:'utf8'}).trim(),sourceCommitIsComplete:!execFileSync('git',['status','--porcelain'],{cwd:root,encoding:'utf8'}).trim(),unpublished:true,files:rows,sourceManifestSha256:manifestSha256,archive,archiveSha256:createHash('sha256').update(await fs.readFile(path.join(output,archive))).digest('hex')};
await fs.writeFile(path.join(output,'source-receipt.json'),JSON.stringify(receipt,null,2)+'\n',{flag:'wx'});console.log(JSON.stringify({archive,files:rows.length,archiveSha256:receipt.archiveSha256}));
